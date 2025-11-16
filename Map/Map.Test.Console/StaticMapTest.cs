using Kernel.Core.EventBus;
using Kernel.Core.TransformTree;
using Kernel.Contract.Geometry;
using Kernel.Contract.Navigation;
using Kernel.Contract.Sensor;
using Kernel.Contract.Tf;
using Map;
using TlarcRosBridge.Infrastructure.Messages.Geometry;
using TlarcRosBridge.Infrastructure.Messages.Nav;
using TlarcRosBridge.Infrastructure.Messages.Sensor;
using TlarcRosBridge.Infrastructure.Messages.Tf2;
using Quaternion = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;
using CostMap.Infrastructure.Data;

public static class StaticMapTest
{
    public static void Build()
    {
        #region Prepare
        Quaternion QuaternionFromYawPitchRoll(float yaw = 0, float pitch = 0, float roll = 0) =>
                Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)(yaw / 180 * Math.PI)) *
                Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)(pitch / 180 * Math.PI)) *
                Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)(roll / 180 * Math.PI));

        const string TfCostMapLinkName  = "cost_map_link";
        const string TfCarLinkName      = "car_link";
        const string TfCarInitName      = "car_init";
        const string TfLidarLinkName    = "lidar_link";
        const string TfLidarInitName    = "lidar_init";

        // Vector3 TfCostMapLinkTranslate = new(-5, -10, 0);
        Vector3 TfCostMapLinkTranslate = new(0, 0, 0);
        Quaternion TfCostMapLinkRotation = Quaternion.Identity;
        // Vector3 TfLidarLinkTranslate = new(0.14f, 0.12f, 0.27f);
        // Quaternion TfLidarLinkRotation = QuaternionFromYawPitchRoll(-49.7f, roll: -50.0f);
        Vector3 TfLidarLinkTranslate = new(0.233f, 0, 0.201f);
        Quaternion TfLidarLinkRotation = QuaternionFromYawPitchRoll(0,10,0);

        // Events
        const string EventPointCloudInputName   = "/tlarc/map_server/point_cloud";
        const string EventRobotPositionName     = "/tlarc/map_server/sensor_pose";
        const string EventGridMapName           = "/tlarc/map/cost_map_with_pcd";
        const string EventTfName                = "/tlarc/tf/publish";
        const string EventPointCloudOutputName  = "/tlarc/point_cloud";


        // Others
        const string PointCloudInputId          = PointCloudTo2dMap.ROGMapDefaultConfig.TfLidarLink;
        const string PointCloudOutputId         = PointCloudTo2dMap.ROGMapDefaultConfig.TfMapLink;
        const string RobotPositionInputTfId     = PointCloudTo2dMap.ROGMapDefaultConfig.TfCarLink;
        #endregion

        #region //ROS
#if true
        const string RosNodeName                            = "TlarcMapServer";
        const string RosSubRegisteredPointCloudTopicName    = "/rmcs_slam/cloud_registered_world";
        const string RosSubRobotPosTopicName                = "/rmcs_slam/pose";
        const string RosPubDebugTlarcGridMapTopicName       = "/tlarc/map/cost_map_with_pcd";
        const string RosPubDebugTlarcTfTopicName            = "/tlarc_tf";
        const string RosPubTlarcPointCloudTopicName         = "/tlarc/point_cloud";

        var ros = TlarcRosBridge.Domain.RosBridge.Build(RosNodeName);

        ros.Subscript<PointCloud2, Kernel.Contract.Sensor.PointCloud>(
            RosSubRegisteredPointCloudTopicName, EventPointCloudInputName,
            TlarcRosBridge.Infrastructure.DataProcess.Subscriber.XYZPointCloud);
        ros.Subscript<PoseStamped, Kernel.Contract.Geometry.Pose>(
            RosSubRobotPosTopicName, EventRobotPositionName,
            TlarcRosBridge.Infrastructure.DataProcess.Subscriber.PoseFromPoseStamped);
        ros.Publish<GridMap2DData, OccupancyGrid>(
            EventGridMapName,
            RosPubDebugTlarcGridMapTopicName,
            TlarcRosBridge.Infrastructure.DataProcess.Publisher.GridMap2dToOccupancyGridMap);
        ros.Publish<GridMap2DData, OccupancyGrid>(
            $"{EventGridMapName}_static",
            $"{RosPubDebugTlarcGridMapTopicName}_static",
            TlarcRosBridge.Infrastructure.DataProcess.Publisher.GridMap2dToOccupancyGridMap);
        ros.Publish<TfCollection, TFMessage>(
            EventTfName,
            RosPubDebugTlarcTfTopicName,
            TlarcRosBridge.Infrastructure.DataProcess.Publisher.TfCollectionToTfMessage);
        ros.Publish<Kernel.Contract.Sensor.PointCloud, PointCloud2>(
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
        Tf.SetTfNode(TfLidarLinkName, TfLidarLinkTranslate, TfLidarLinkRotation);
        EventBus<TfCollection>.Instance.Publish(EventTfName, Tf.GetTree());

        #endregion



        var rog = PointCloudTo2dMap.ROGMapDefault.BuildROGMap();
        rog.Visualize = true;
        var avg = AverageOGM.CreateEmpty(new()
        {
            Header = new(){Identifier = TfCarInitName},
            Height = 1000,
            Width = 1000,
            Resolution = 0.02f,
            Origin = new(-10,-10)
        });
    
        EventBus<Kernel.Contract.Geometry.Pose>.Instance.Subscribe(EventRobotPositionName,
            data =>
            {
                Tf.SetTfNode(RobotPositionInputTfId, data.Translation, data.Orientation, data.Header.Timestamp.ToStamp);
                EventBus<TfCollection>.Instance.Publish(EventTfName, Tf.GetTree());
            });

        EventBus<Kernel.Contract.Sensor.PointCloud>.Instance.Subscribe(EventPointCloudInputName,
            data =>
            {
                Kernel.Contract.Sensor.PointCloud pointCloud = new()
                {
                    Points = Tf.Cast(PointCloudInputId, PointCloudOutputId, data.Points, new Vector3[data.Points.Length], data.Header.Timestamp.ToStamp),
                    Header = new() { Identifier = PointCloudOutputId }
                };
                EventBus<Kernel.Contract.Sensor.PointCloud>.Instance.Publish(EventPointCloudOutputName, pointCloud);
            });

        EventBus<Kernel.Contract.Geometry.Pose>.Instance.Subscribe(EventRobotPositionName,
            data =>
            {
                Tf.SetTfNode(RobotPositionInputTfId, data.Translation, data.Orientation, data.Header.Timestamp.ToStamp);
                EventBus<TfCollection>.Instance.Publish(EventTfName, Tf.GetTree());
            });
        EventBus<ROGMap>.Instance.Subscribe(EventGridMapName,
            data =>
            {
                avg.Update(data, x => Tf.Cast(data.Header.Identifier,avg._description.Header.Identifier, x));
                EventBus<GridMap2DData>.Instance.Publish($"{EventGridMapName}_static", (GridMap2DData)avg);
            });
        Console.WriteLine("Launch up");
    }
}