using Kernel.Core.EventBus;
using Kernel.Core.TransformTree;
using Kernel.DataInterfaces.Geometry;
using Kernel.DataInterfaces.Navigation;
using Kernel.DataInterfaces.Sensor;
using Kernel.DataInterfaces.Tf;
using Map;
using TlarcRosBridge.Infrastructure.Messages.Geometry;
using TlarcRosBridge.Infrastructure.Messages.Nav;
using TlarcRosBridge.Infrastructure.Messages.Sensor;
using TlarcRosBridge.Infrastructure.Messages.Tf2;
using Quaternion = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;

#region Prepare
Quaternion QuaternionFromYawPitchRoll(float yaw = 0, float pitch = 0, float roll = 0) =>
        Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)(yaw / 180 * Math.PI)) *
        Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)(pitch / 180 * Math.PI)) *
        Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)(roll / 180 * Math.PI));

const string TfCostMapLinkName = "cost_map_link";
const string TfCarLinkName = "car_link";
const string TfCarInitName = "car_init";
const string TfLidarLinkName = "lidar_link";
const string TfLidarInitName = "lidar_init";

Vector3 TfCostMapLinkTranslate = new(-5, -10, 0);
Quaternion TfCostMapLinkRotation = Quaternion.Identity;
Vector3 TfLidarLinkTranslate = new(0.14f, 0.12f, 0.27f);
Quaternion TfLidarLinkRotation = QuaternionFromYawPitchRoll(-49.7f, roll: -50.0f);

// Events
const string EventPointCloudInputName = "/tlarc/map_server/point_cloud";
const string EventRobotPositionName = "/tlarc/map_server/sensor_pose";
const string EventGridMapName = "/tlarc/map/cost_map_with_pcd";
const string EventTfName = "/tlarc/tf/publish";
const string EventPointCloudOutputName = "/tlarc/point_cloud";


// Others
const string PointCloudInputId = TfCarInitName;
const string PointCloudOutputId = TfLidarLinkName;
const string PointCloudCostMapSensorId = TfLidarLinkName;
const string RobotPositionInputTfId = TfCarLinkName;
#endregion

#region //ROS
#if true 
const string RosNodeName = "TlarcMapServer";
const string RosSubRegisteredPointCloudTopicName = "/rmcs_slam/cloud_registered_world";
const string RosSubRobotPosTopicName = "/rmcs_slam/pose";
const string RosPubDebugTlarcGridMapTopicName = "/tlarc/map/cost_map_with_pcd";
const string RosPubDebugTlarcTfTopicName = "/tlarc_tf";
const string RosPubTlarcPointCloudTopicName = "/tlarc/point_cloud";

var ros = TlarcRosBridge.Domain.RosBridge.Build(RosNodeName);

ros.Subscript<PointCloud2, IPointCloud>(
    RosSubRegisteredPointCloudTopicName, EventPointCloudInputName,
    TlarcRosBridge.Infrastructure.DataProcess.Subscriber.FastLioRegistered);
ros.Subscript<PoseStamped, IPose>(
    RosSubRobotPosTopicName, EventRobotPositionName,
    TlarcRosBridge.Infrastructure.DataProcess.Subscriber.RawPoseFromPoseStamped);
ros.Publish<IGridMap2DData, OccupancyGrid>(
    EventGridMapName,
    RosPubDebugTlarcGridMapTopicName,
    TlarcRosBridge.Infrastructure.DataProcess.Publisher.GridMap2dToOccupancyGridMap);
ros.Publish<ITfCollection, TFMessage>(
    EventTfName,
    RosPubDebugTlarcTfTopicName,
    TlarcRosBridge.Infrastructure.DataProcess.Publisher.TfCollectionToTfMessage);
ros.Publish<IPointCloud, PointCloud2>(
    EventPointCloudOutputName,
    RosPubTlarcPointCloudTopicName,
    TlarcRosBridge.Infrastructure.DataProcess.Publisher.PublishPointCloud);



#endif
#endregion

#region TFSetupHere

Tf.AddTfNode(TfCostMapLinkName, TfCarInitName);
Tf.AddTfNode(TfCarLinkName, TfCarInitName);
Tf.AddTfNode(TfLidarInitName, TfCarInitName);
Tf.AddTfNode(TfLidarLinkName, TfCarLinkName);
Tf.SetTfNode(TfCostMapLinkName, TfCostMapLinkTranslate, TfCostMapLinkRotation);
Tf.SetTfNode(TfLidarLinkName, TfLidarLinkTranslate, Quaternion.Identity);
EventBus<ITfCollection>.Instance.Publish(EventTfName, Tf.GetTree());

#endregion


EventBus<IPose>.Instance.Subscribe(EventRobotPositionName,
    data =>
    {
        Tf.SetTfNode(RobotPositionInputTfId, data.Position, data.Orientation);
        EventBus<ITfCollection>.Instance.Publish(EventTfName, Tf.GetTree());
    });

EventBus<IPointCloud>.Instance.Subscribe(EventPointCloudInputName,
    data =>
    {
        Kernel.DataInterfaces.Sensor.PointCloud pointCloud = new()
        {
            Points = Tf.Cast(PointCloudInputId, PointCloudOutputId, data.Points),
            Identifier = PointCloudOutputId
        };
        EventBus<IPointCloud>.Instance.Publish(EventPointCloudOutputName, pointCloud);
    });

GC.KeepAlive(
    PointCloudTo2dMap.DefaultNew
        .SetInput_PointCloudTopicName(EventPointCloudInputName)
        .SetOutput_DataStructure(width: 600, height: 600, resolution: 0.02f, topZ: 0.7f, bottomZ: -0.1f, lossFree: 0.7f, lossOccu: -4.7f,
                                    blindCircleRadius: 0.4f, slidingThreshold: 1, forgetFrameCount: 60, highError: 0.1f, OccupyDensity: 0.5f, inflationRadius: 0.1f)
        .SetOutput_CostMapTopicName(EventGridMapName)
        .SetOutput_Inflation(radius: 20)
        .SetId_PointCloud(PointCloudInputId)
        .SetId_Sensor(PointCloudCostMapSensorId)
        .SetId_Map(TfCostMapLinkName)
        .SetId_Chassis(TfCarLinkName)
        .SetId_Odom(TfCarInitName)
        .BuildROGMap()
);
Console.WriteLine("Launch up");
while (true) ;