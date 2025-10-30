
using System.Numerics;
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
using Planner;
using TlarcRosBridge.Infrastructure.Messages.Geometry;
using TlarcRosBridge.Infrastructure.Messages.Nav;
using TlarcRosBridge.Infrastructure.Messages.Visualization;
public static class CircleObstacle
{

    const string RosNodeName = "TlarcMapServer";
    const string RosStaticMap = "/tlarc/static_map";
    const string RosStaticInflationMap = "/tlarc/static_map_inflation";
    const string RosSafeCorridorName = "/tlarc/safe_corridor";
    const string RosControlPoint = "/tlarc/point/contorl";
    const string RosPositionPoint = "/transform/sentry/publish";
    const string RosPathTopic = "/tlarc/astar/path";
    const string RosTfTopic = "/tlarc/tf";
    const string RosTargetVelocityTopic = "/tlarc/target_velocity";
    const string RosTrajectoryTopic = "/tlarc/minco/trajectory";
    public static void Build()
    {
        MapLoader loader =
               MapLoader.Default
               .SetMapPath("~/Download/Tlarc/Maps/misaka/")
               .LoadMap();


        Tf.AddTfNode(loader.MapFrame, "car_init");

        var obs = ObstacleMap.Default.SetEventGridMapName(loader.MapEventName).BuildMapRelatedMap();
        var planner = new Planner.PlannerBuilder<CostMap.Infrastructure.Data.ROGMap>()
                            .SetMapHeight((int)loader.MapHeight)
                            .SetMapWidth((int)loader.MapWidth)
                            .SetMapResolution(loader.MapResolution)
                            .SetPathSearchIteratorStep(0.3f)
                            .SetGridMapEventName(loader.MapEventName)
                            .SetObstacleMapEventName(obs.EventObstacleName)
                            .BuildALPlanner();

        Console.WriteLine("Map Load");

#if true

        var ros = TlarcRosBridge.Domain.RosBridge.Build(RosNodeName);

        ros.Publish<GridMap2DData, OccupancyGrid>(
             loader.MapEventName, RosStaticMap,
            TlarcRosBridge.Infrastructure.DataProcess.Publisher.GridMap2dToOccupancyGridMap);
        ros.Publish<Path2D, TlarcRosBridge.Infrastructure.Messages.Nav.Path>(RosTrajectoryTopic, RosTrajectoryTopic
                            , TlarcRosBridge.Infrastructure.DataProcess.Publisher.PublishPath);
        ros.Publish<GridMap2DData, OccupancyGrid>(
             RosStaticInflationMap, RosStaticInflationMap,
            TlarcRosBridge.Infrastructure.DataProcess.Publisher.GridMap2dToOccupancyGridMap);
        ros.Publish<SafeCorridor2DData<Circle>, MarkerArray>(
             RosSafeCorridorName, RosSafeCorridorName,
            TlarcRosBridge.Infrastructure.DataProcess.Publisher.PublishCircleSafeCorridor);
        ros.Publish<Path2D, TlarcRosBridge.Infrastructure.Messages.Nav.Path>(
             RosPathTopic, RosPathTopic,
            TlarcRosBridge.Infrastructure.DataProcess.Publisher.PublishPath);
        ros.Publish<TfCollection, TlarcRosBridge.Infrastructure.Messages.Tf2.TFMessage>(
             RosTfTopic, RosTfTopic,
            TlarcRosBridge.Infrastructure.DataProcess.Publisher.TfCollectionToTfMessage);
        ros.Publish<Kernel.Contract.Geometry.Pose, TlarcRosBridge.Infrastructure.Messages.Geometry.PoseStamped>(
             RosTargetVelocityTopic, RosTargetVelocityTopic,
            TlarcRosBridge.Infrastructure.DataProcess.Publisher.PublishPoseStamped);
#endif
        InflationLayerBuilder.SetPara(20);

        EventBus<GridMap2DData>.Instance.Subscribe(loader.MapEventName, x =>
        {
            var infmap = InflationLayerBuilder.Build(Grid2DMap.Build_GridMap2DData(x), x);
            EventBus<GridMap2DData>.Instance.Publish(RosStaticInflationMap, infmap.GridMap.Data);
        }
        );

        Vector2 vel = Vector2.Zero;
        bool reload = true;
        Kernel.Contract.Geometry.Pose2D from = new();
        Kernel.Contract.Geometry.Pose2D to = new();
        EventBus<Kernel.Contract.Geometry.Pose2D>.Instance.Subscribe(RosControlPoint, x =>
        {
            to = x;

            reload = true;
        });

        ITrajectory2D? lasttraj = null;
        EventBus<Kernel.Contract.Geometry.Pose>.Instance.Subscribe(RosPositionPoint, x =>
        {
            if (lasttraj is not null)
            {
                vel = lasttraj?.GetVelocity(DateTime.UtcNow) ?? Vector2.Zero;
                EventBus<Kernel.Contract.Geometry.Pose>.Instance.Publish(RosTargetVelocityTopic, new Kernel.Contract.Geometry.Pose(){ 
                        Translation = new(vel, 0), 
                        Orientation = System.Numerics.Quaternion.Zero, 
                        Header = lasttraj!.Data.Header});
            }
            if (!reload)
                return;
            reload = false;
            from = new  (){Header = x.Header, Orientation = 0,Translation = new(x.Translation.X,x.Translation.Y)};
            var astar = planner.SeachPath(from, to);
            EventBus<Path2D>.Instance.Publish(RosPathTopic, astar);
            var safeCorridor = planner.SearchCircleSafeCorridor(astar);

            EventBus<SafeCorridor2DData<Circle>>.Instance.Publish(RosSafeCorridorName, safeCorridor);


            var trajectory =
                PlannerBuilder<CostMap.Infrastructure.Data.ROGMap>.OptimizePath
                (
                    safeCorridor!,
                    new(safeCorridor!.Corridors[0].Origin, vel, Vector2.Zero),
                    new(safeCorridor!.Corridors[^1].Origin, Vector2.Zero, Vector2.Zero)
                );

            if (trajectory is not null)
                EventBus<Path2D>
                    .Instance.Publish(
                        RosTrajectoryTopic,
                        new Path2D{
                            Points =[.. trajectory.GetPositions(trajectory.Data.FromWhen, (trajectory.Data.ToWhen - trajectory.Data.FromWhen).TotalSeconds / 100f, 101)],
                            Header = new() { Identifier = "test"}
                        });

            lasttraj = trajectory ?? lasttraj;

        });
        loader.MapPublish();
        EventBus<TfCollection>.Instance.Publish(RosTfTopic, Tf.GetTree());
        Console.WriteLine("Map publish");


        ros.Subscript<PointStamped, StdMessage<Vector2>>(
             RosControlPoint, RosControlPoint,
            TlarcRosBridge.Infrastructure.DataProcess.Subscriber.PointToVector2);
        ros.Subscript<PoseStamped, Kernel.Contract.Geometry.Pose>(
             RosPositionPoint, RosPositionPoint,
            TlarcRosBridge.Infrastructure.DataProcess.Subscriber.RawPoseFromPoseStamped);
    }
}