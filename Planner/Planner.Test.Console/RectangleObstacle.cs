
using System.Numerics;
using ALPlanner.Infrastructure.SafeCorridorConstruct;
using CostMap.Infrastructure.Algorithm;
using CostMap.Infrastructure.Data;
using Kernel.Core.EventBus;
using Kernel.Core.Messages;
using Kernel.Core.TransformTree;
using Kernel.Contract.Constraints;
using Kernel.Contract.Geometry;
using Kernel.Contract.Navigation;
using Kernel.Contract.Tf;
using Kernel.Contract.Visualization;
using Map;
using TlarcRosBridge.Infrastructure.Messages.Geometry;
using TlarcRosBridge.Infrastructure.Messages.Nav;
using TlarcRosBridge.Infrastructure.Messages.Visualization;
using TlarcRosBridge.Infrastructure.Messages.Sensor;
using Vector3 = System.Numerics.Vector3;
using Quaternion = System.Numerics.Quaternion;
using TlarcRosBridge.Infrastructure.Messages.Tf2;
public static class RectAngleObstacle
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

        #region TFSetupHere

        Tf.AddTfNode(TfCostMapLinkName, TfCarInitName);
        Tf.AddTfNode(TfCarLinkName, TfCarInitName);
        Tf.AddTfNode(TfLidarInitName, TfCarInitName);
        Tf.AddTfNode(TfLidarLinkName, TfCarLinkName);
        Tf.SetTfNode(TfCostMapLinkName, TfCostMapLinkTranslate, TfCostMapLinkRotation);
        Tf.SetTfNode(TfLidarLinkName, TfLidarLinkTranslate, TfLidarLinkRotation);
        #endregion

        #region ROS
#if true
        const string RosNodeName                            = "TlarcMapServer";
        const string RosSubRegisteredPointCloudTopicName    = "/rmcs_slam/cloud_registered_world";
        const string RosSubRobotPosTopicName                = "/rmcs_slam/pose";
        const string RosPubDebugTlarcGridMapTopicName       = "/tlarc/map/cost_map_with_pcd";
        const string RosPubDebugTlarcTfTopicName            = "/tlarc_tf";
        const string RosPubTlarcPointCloudTopicName         = "/tlarc/point_cloud";
        const string RosPathTopic                           = "/tlarc/path";
        const string RosControlPoint                        = "/tlarc/point/contorl";

        var ros = TlarcRosBridge.Domain.RosBridge.Build(RosNodeName);

        ros.Publish<GridMap2DData, OccupancyGrid>(
            EventGridMapName,
            RosPubDebugTlarcGridMapTopicName,
            TlarcRosBridge.Infrastructure.DataProcess.Publisher.GridMap2dToOccupancyGridMap);
        ros.Publish<TfCollection, TFMessage>(
            EventTfName,
            RosPubDebugTlarcTfTopicName,
            TlarcRosBridge.Infrastructure.DataProcess.Publisher.TfCollectionToTfMessage);
        ros.Publish<Kernel.Contract.Sensor.PointCloud, PointCloud2>(
            EventPointCloudOutputName,
            RosPubTlarcPointCloudTopicName,
            TlarcRosBridge.Infrastructure.DataProcess.Publisher.PublishPointCloud);
        ros.Publish<Kernel.Contract.Navigation.Path2D, TlarcRosBridge.Infrastructure.Messages.Nav.Path>(
            RosPathTopic,
            RosPathTopic,
            TlarcRosBridge.Infrastructure.DataProcess.Publisher.PublishPath);
#endif
        #endregion

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

        var map = PointCloudTo2dMap.ROGMapDefault.BuildROGMap();
        map.Visualize = true;
        var planner = new Planner.PlannerBuilder<CostMap.Infrastructure.Data.ROGMap>()
            .SetMapHeight(600)
            .SetMapWidth(600)
            .SetMapResolution(0.02f)
            .SetPathSearchIteratorStep(0.3f)
            .SetGridMapEventName(EventGridMapName)
            .SetMap(map) 
            .BuildALPlanner();
        planner.Identifier = PointCloudTo2dMap.ROGMapDefaultConfig.TfOdomLink; 
        // InflationLayerBuilder.SetPara(50);

        Vector2 vel = Vector2.Zero;
        bool reload = true;
        Kernel.Contract.Geometry.Pose2D from = new();
        Kernel.Contract.Geometry.Pose2D to = new()
        {
            Header = new Kernel.Contract.Header() { Identifier = PointCloudTo2dMap.ROGMapDefaultConfig.TfOdomLink }
        };
        EventBus<StdMessage<Vector2>>.Instance.Subscribe(RosControlPoint, x =>
        {
            to.Translation = x.Instance;
            reload = true;
        });

        ITrajectory2D? lasttraj = null;
        EventBus<Kernel.Contract.Geometry.Pose>.Instance.Subscribe(EventRobotPositionName, x =>
        {
            if (lasttraj is not null)
            {
                vel = lasttraj?.GetVelocity(DateTime.UtcNow) ?? Vector2.Zero;
                // EventBus<Kernel.Contract.Geometry.Pose>.Instance.Publish(RosTargetVelocityTopic, new(){ Position = new(vel, 0),Orientation = System.Numerics.Quaternion.Zero, Header = lasttraj!.Data.Header});
            }
            if (!reload)
                return;
            reload = false;
            from = new(){Header = x.Header, Orientation = 0,Translation = new(x.Translation.X,x.Translation.Y)};
            var astar = planner.SeachPath(from, to);
            EventBus<Path2D>.Instance.Publish(RosPathTopic, astar);
            // var safeCorridor = planner.SearchAABBSafeCorridor(astar);


            // var trajectory = Planner.PlannerBuilder<ROGMap>.OptimizePath(
            //     safeCorridor!,
            //     new(astar.Points[0] , vel, Vector2.Zero),
            //     new(astar.Points[^1], Vector2.Zero, Vector2.Zero));
            // if (trajectory is not null) EventBus<Path2D>.Instance.Publish(RosTrajectoryTopic,
            //  new Path2D(){  Points = [.. trajectory.GetPositions(trajectory.Data.FromWhen, (trajectory.Data.ToWhen - trajectory.Data.FromWhen).TotalSeconds / 100f, 101)],
            //                 Header = trajectory.Data.Header});
            // lasttraj = trajectory ?? lasttraj;

        });
        Console.WriteLine("Map publish");


        ros.Subscript<PointCloud2, Kernel.Contract.Sensor.PointCloud>(
            RosSubRegisteredPointCloudTopicName, EventPointCloudInputName,
            TlarcRosBridge.Infrastructure.DataProcess.Subscriber.XYZPointCloud);
        ros.Subscript<PoseStamped, Kernel.Contract.Geometry.Pose>(
            RosSubRobotPosTopicName, EventRobotPositionName,
            TlarcRosBridge.Infrastructure.DataProcess.Subscriber.PoseFromPoseStamped);
        ros.Subscript<PointStamped, StdMessage<Vector2>>(
             RosControlPoint, RosControlPoint,
            TlarcRosBridge.Infrastructure.DataProcess.Subscriber.PointToVector2);
    }
}