
using System.Numerics;
using CostMap.Infrastructure.Algorithm;
using CostMap.Infrastructure.Data;
using Kernel.Core.EventBus;
using Kernel.Core.Messages;
using Kernel.Core.TransformTree;
using Kernel.DataInterfaces.Constraints;
using Kernel.DataInterfaces.Geometry;
using Kernel.DataInterfaces.Navigation;
using Kernel.DataInterfaces.Tf;
using Kernel.DataInterfaces.Visualization;
using Map;
using TlarcRosBridge.Infrastructure.Messages.Geometry;
using TlarcRosBridge.Infrastructure.Messages.Nav;
using TlarcRosBridge.Infrastructure.Messages.Visualization;

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

MapLoader loader =
        MapLoader.Default
        .SetMapPath("~/Download/Tlarc/Maps/misaka/")
        .LoadMap();


Tf.AddTfNode(loader.MapFrame, "car_init");

var obs = ObstacleMap.Default.SetEventSdfMapName(loader.MapEventName).BuildMapRelatedMap();
var planner = new Planner.PlannerBuilder()
                    .SetMapHeight((int)loader.MapHeight)
                    .SetMapWidth((int)loader.MapWidth)
                    .SetMapResolution(loader.MapResolution)
                    .SetPathSearchIteratorStep(0.3f)
                    .SetSdfMapEventName(loader.MapEventName)
                    .SetObstacleMapEventName(obs.EventObstacleName)
                    .BuildALPlanner();

Console.WriteLine("Map Load");

#if true

var ros = TlarcRosBridge.Domain.RosBridge.Build(RosNodeName);

ros.Publish<IGridMap2DData, OccupancyGrid>(
     loader.MapEventName, RosStaticMap,
    TlarcRosBridge.Infrastructure.DataProcess.Publisher.GridMap2dToOccupancyGridMap);
ros.Publish<IPath2D, TlarcRosBridge.Infrastructure.Messages.Nav.Path>(RosTrajectoryTopic, RosTrajectoryTopic
                    , TlarcRosBridge.Infrastructure.DataProcess.Publisher.PublishPath);
ros.Publish<IGridMap2DData, OccupancyGrid>(
     RosStaticInflationMap, RosStaticInflationMap,
    TlarcRosBridge.Infrastructure.DataProcess.Publisher.GridMap2dToOccupancyGridMap);
ros.Publish<ISafeCorridor2DData<ICircle>, MarkerArray>(
     RosSafeCorridorName, RosSafeCorridorName,
    TlarcRosBridge.Infrastructure.DataProcess.Publisher.PublishCircleSafeCorridor);
ros.Publish<IPath2D, TlarcRosBridge.Infrastructure.Messages.Nav.Path>(
     RosPathTopic, RosPathTopic,
    TlarcRosBridge.Infrastructure.DataProcess.Publisher.PublishPath);
ros.Publish<ITfCollection, TlarcRosBridge.Infrastructure.Messages.Tf2.TFMessage>(
     RosTfTopic, RosTfTopic,
    TlarcRosBridge.Infrastructure.DataProcess.Publisher.TfCollectionToTfMessage);
ros.Publish<IPose, TlarcRosBridge.Infrastructure.Messages.Geometry.PoseStamped>(
     RosTargetVelocityTopic, RosTargetVelocityTopic,
    TlarcRosBridge.Infrastructure.DataProcess.Publisher.PublishPoseStamped);
#endif
InflationLayerBuilder.SetPara(50);

EventBus<IGridMap2DData>.Instance.Subscribe(loader.MapEventName, x =>
{
    var infmap = InflationLayerBuilder.Build(Grid2DMap.Build_IGridMap2DData(x));
    EventBus<ISdf2D>.Instance.Publish(loader.MapEventName, infmap);
    EventBus<IGridMap2DData>.Instance.Publish(RosStaticInflationMap, infmap.GridMap.Data);
}
);

Vector2 vel = Vector2.Zero;
bool reload = true;
Vector2 from = new();
Vector2 to = new();
EventBus<StdMessage<Vector2>>.Instance.Subscribe(RosControlPoint, x =>
{
    to = x.Instance;

    reload = true;
});

ITrajectory2D? lasttraj = null;
EventBus<IPose>.Instance.Subscribe(RosPositionPoint, x =>
{
    if (lasttraj is not null)
    {
        vel = lasttraj?.GetVelocity(DateTime.UtcNow) ?? Vector2.Zero;
        EventBus<IPose>.Instance.Publish(RosTargetVelocityTopic, new PoseDecoratior(new(vel, 0), System.Numerics.Quaternion.Zero, lasttraj.Header));
    }
    if (!reload)
        return;
    reload = false;
    from = new(x.Position.X, x.Position.Y);
    var astar = planner.SeachPath(from, to);
    EventBus<IPath2D>.Instance.Publish(RosPathTopic, astar);
    var safeCorridor = planner.SearchSafeCorridor(astar);
    // EventBus<ISafeCorridor2DData<ICircle>>.Instance.Publish(RosSafeCorridorName, safeCorridor);


    var trajectory = planner.OptimizePath(
        safeCorridor!,
        new(safeCorridor!.Corridors[0].Origin, vel, Vector2.Zero),
        new(safeCorridor!.Corridors[^1].Origin, Vector2.Zero, Vector2.Zero));
    if (trajectory is not null) EventBus<IPath2D>.Instance.Publish(RosTrajectoryTopic,
     new PathDecorator<IEnumerable<Vector2>>(trajectory.GetPositions(trajectory.FromWhen, (trajectory.ToWhen - trajectory.FromWhen).TotalSeconds / 100f, 101), trajectory.Header));
    lasttraj = trajectory ?? lasttraj;

});
loader.MapPublish();
EventBus<ITfCollection>.Instance.Publish(RosTfTopic, Tf.GetTree());
Console.WriteLine("Map publish");


ros.Subscript<PointStamped, StdMessage<Vector2>>(
     RosControlPoint, RosControlPoint,
    TlarcRosBridge.Infrastructure.DataProcess.Subscriber.PointToVector2);
ros.Subscript<PoseStamped, IPose>(
     RosPositionPoint, RosPositionPoint,
    TlarcRosBridge.Infrastructure.DataProcess.Subscriber.RawPoseFromPoseStamped);
