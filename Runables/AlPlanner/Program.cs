// See https://aka.ms/new-console-template for more information

using Map;
using Kernel.Core.EventBus;
using Kernel.Core.Messages;
using Kernel.Core.TransformTree;
using Kernel.DataInterfaces.Geometry;
using Kernel.DataInterfaces.Navigation;
using Kernel.DataInterfaces.Sensor;
using TlarcRosBridge.Infrastructure.Messages.Geometry;
using TlarcRosBridge.Infrastructure.Messages.Nav;
using TlarcRosBridge.Infrastructure.Messages.Sensor;
using Kernel.DataInterfaces.Tf;
using TlarcRosBridge.Domain;
using TlarcRosBridge.Infrastructure.Messages.Tf2;
using Quaternion = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;


#region Parameters

// TF
Quaternion QuaternionFromYawPitchRoll(float yaw = 0, float pitch = 0, float roll = 0) =>
    Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)(yaw / 180 * Math.PI)) *
    Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)(pitch / 180 * Math.PI)) *
    Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)(roll / 180 * Math.PI));

const string TfCostMapLinkName = "cost_map_link";
const string TfCarLinkName = "car_link";
const string TfCarInitName = "world_link";
const string TfLidarLinkName = "lidar_link";
const string TfSensorLidar1LinkName = "sensor_lidar_1_link";
const string TfStaticMapName = "car_init";
Vector3 TfCostMapLinkTranslate = new(-5f, -5f, 0);
var TfCostMapLinkRotation = Quaternion.Identity;
Vector3 TfLidarLinkTranslate = new(0.14f, 0.12f, 0.27f);
var TfLidarLinkRotation = QuaternionFromYawPitchRoll(-49.7f, roll: -50.0f);


// Events
const string EventPointCloudInputName = "/tlarc/map_server/point_cloud";
const string EventRobotPositionName = "/tlarc/map_server/sensor_pose";
const string EventGridMapName = "/tlarc/map/cost_map_with_pcd";
const string EventTfName = "/tlarc/tf/publish";
const string EventPointCloudOutputName = "/tlarc/point_cloud";
const string EventSaveMapName = "/tlarc/save_map";

// Ros
const string RosNodeName = "TlarcMapServer";
const string RosSubRegisteredPointCloudTopicName = "/livox/lidar_192_168_100_120/undistort";
const string RosSubRobotPosTopicName = "/rmcs_slam/pose";
const string RosPubDebugTlarcGridMapTopicName = "/tlarc/map/cost_map_with_pcd";
const string RosPubDebugTlarcTfTopicName = "/tlarc_tf";
const string RosPubTlarcPointCloudTopicName = "/tlarc/recast/point_cloud";

// Others
const string PointCloudInputId = TfLidarLinkName;
const string PointCloudOutputId = TfCostMapLinkName;
const string PointCloudCostMapSensorId = TfSensorLidar1LinkName;
const string RobotPositionInputTfId = TfCarLinkName;

const string staticMapPath = "~/Download/Tlarc/Maps/TestHigh";
const string staticMapRosTopicName = "/tlarc/map_server/static_map";
const string staticMapEventName = "/tlarc/map_server/static_map";

#endregion

#region TF构建

Tf.AddTfNode(TfCostMapLinkName, TfCarLinkName);
Tf.AddTfNode(TfCarLinkName, TfCarInitName);
Tf.AddTfNode(TfStaticMapName, TfCarInitName);
Tf.AddTfNode(TfLidarLinkName, TfCarLinkName);
Tf.AddTfNode(TfSensorLidar1LinkName, TfCarLinkName);
Tf.SetTfNode(TfCostMapLinkName, TfCostMapLinkTranslate, TfCostMapLinkRotation);
Tf.SetTfNode(TfSensorLidar1LinkName, TfLidarLinkTranslate, Quaternion.Identity);

#endregion

#region ROS

var ros = RosBridge.Build("Tlarc");
ros.Publish<IGridMap2DData, OccupancyGrid>
(staticMapEventName, staticMapRosTopicName,
    TlarcRosBridge.Infrastructure.DataProcess.Publisher.GridMap2dToOccupancyGridMap);
ros.Publish<ITfCollection, TFMessage>(
    EventTfName,
    RosPubDebugTlarcTfTopicName,
    TlarcRosBridge.Infrastructure.DataProcess.Publisher.TfCollectionToTfMessage);
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

#endregion

#region Main

var pcdStaticMap =
    PointCloudTo2dMap.DefaultNew
        .SetInput_PointCloudTopicName(EventPointCloudInputName)
        .SetInput_StaticMapTopicName(staticMapEventName)
        .SetOutput_DataStructure(
            250, 250,
            resolution: 0.04f, topZ: 0.3f, bottomZ: -0.3f,
            lossFree: 0.7f, lossOccu: -0.9f)
        .SetId_PointCloud(PointCloudInputId)
        .SetId_Sensor(PointCloudCostMapSensorId)
        .BuildOccupancyMapWithStaticHigh();
var loader = MapLoader.Default
    .SetMapPath(staticMapPath)
    .SetEventName(staticMapEventName)
    .HighMapPublish();

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
            Points = Tf.Cast(PointCloudInputId, PointCloudOutputId, data.Points, new Vector3[data.Points.Length]),
            Identifier = PointCloudOutputId
        };
        EventBus<IPointCloud>.Instance.Publish(EventPointCloudOutputName, pointCloud);
    });

GC.KeepAlive(pcdStaticMap);
GC.KeepAlive(loader);

#endregion