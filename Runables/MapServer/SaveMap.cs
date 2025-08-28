namespace MapServer;

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
using TlarcRosBridge.Infrastructure.Messages.Tf2;
using Quaternion = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;

public static class SaveMap
{
    #region Parameters 调好了，改这里就好

    // TF
    /*
     * tf tree:
     * car_init
     * |
     * L car_link
     * |    |
     * |    L lidar_link
     * |
     * L cost_map_link
     * |
     * L lidar_init
     */
    private static Quaternion QuaternionFromYawPitchRoll(float yaw = 0, float pitch = 0, float roll = 0) =>
        Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)(yaw / 180 * Math.PI)) *
        Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)(pitch / 180 * Math.PI)) *
        Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)(roll / 180 * Math.PI));

    private const string TfCostMapLinkName = "cost_map_link";
    private const string TfCarLinkName = "car_link";
    private const string TfCarInitName = "car_init";
    private const string TfLidarLinkName = "lidar_link";
    private const string TfLidarInitName = "lidar_init";
    private static readonly Vector3 TfCostMapLinkTranslate = new(-5, -10, 0);
    private static readonly Quaternion TfCostMapLinkRotation = Quaternion.Identity;
    private static readonly Vector3 TfLidarLinkTranslate = new(0.14f, 0.12f, 0.27f);
    private static readonly Quaternion TfLidarLinkRotation = SaveMap.QuaternionFromYawPitchRoll(-49.7f, roll: -50.0f);

    // Events
    private const string EventPointCloudInputName = "/tlarc/map_server/point_cloud";
    private const string EventRobotPositionName = "/tlarc/map_server/sensor_pose";
    private const string EventGridMapName = "/tlarc/map/cost_map_with_pcd";
    private const string EventTfName = "/tlarc/tf/publish";
    private const string EventPointCloudOutputName = "/tlarc/point_cloud";
    private const string EventSaveMapName = "/tlarc/save_map";

    // Ros
    private const string RosNodeName = "TlarcMapServer";
    private const string RosSubRegisteredPointCloudTopicName = "/rmcs_slam/cloud_registered_world";
    private const string RosSubRobotPosTopicName = "/rmcs_slam/pose";
    private const string RosPubDebugTlarcGridMapTopicName = "/tlarc/map/cost_map_with_pcd";
    private const string RosPubDebugTlarcTfTopicName = "/tlarc_tf";
    private const string RosPubTlarcPointCloudTopicName = "/tlarc/point_cloud";

    // Others
    private const string PointCloudInputId = TfCarInitName;
    private const string PointCloudOutputId = TfCostMapLinkName;
    private const string PointCloudCostMapSensorId = TfLidarLinkName;
    private const string RobotPositionInputTfId = TfCarLinkName;

    internal static string MapSavePath = "~/Download/Tlarc/Maps/";


    #endregion

    public static void Build()
    {
        #region TFSetupHere

        Tf.AddTfNode(TfCostMapLinkName, TfCarInitName);
        Tf.AddTfNode(TfCarLinkName, TfCarInitName);
        Tf.AddTfNode(TfLidarInitName, TfCarInitName);
        Tf.AddTfNode(TfLidarLinkName, TfCarLinkName);
        Tf.SetTfNode(TfCostMapLinkName, TfCostMapLinkTranslate, TfCostMapLinkRotation);
        Tf.SetTfNode(TfLidarLinkName, TfLidarLinkTranslate, Quaternion.Identity);

        #endregion

        #region ROS Setup Here

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

        #endregion

        #region Domain Setup Here

        var _pcdStaticMap =
             PointCloudTo2dMap.DefaultNew
                 .SetInput_PointCloudTopicName(EventPointCloudInputName)
                 .SetOutput_DataStructure(750, 450, resolution: 0.04f, topZ: 0.4f, bottomZ: -0.5f, lossFree: 0.7f,
                     lossOccu: -0.9f)
                 .SetId_PointCloud(PointCloudInputId)
                 .SetId_Sensor(PointCloudCostMapSensorId)
                 .BuildOccupancyHighMap();
        var _saver =
            MapSaver.DefaultNew
                .SetInput_CostMapTopicName(EventGridMapName)
                .SetTrigger_SaveTriggerTopicName(EventSaveMapName)
                .Build();

        #endregion

        GC.KeepAlive(_pcdStaticMap);
        GC.KeepAlive(_saver);

        #region MainLogics

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

        #endregion
    }


    public static void SaveTrigger() =>
        EventBus<StringMessage>.Instance.Publish(EventSaveMapName,
            StringMessage.Build(MapSavePath));
}