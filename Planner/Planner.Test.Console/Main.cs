
using System.Numerics;
using CostMap.Infrastructure.Algorithm;
using CostMap.Infrastructure.Data;
using Kernel.Core.EventBus;
using Kernel.Core.Messages;
using Kernel.DataInterfaces.Constraints;
using Kernel.DataInterfaces.Navigation;
using Map;
using TlarcRosBridge.Infrastructure.Messages.Geometry;
using TlarcRosBridge.Infrastructure.Messages.Nav;
using TlarcRosBridge.Infrastructure.Messages.Visualization;

const string RosNodeName = "TlarcMapServer";
const string RosStaticMap = "/tlarc/static_map";
const string RosStaticInflationMap = "/tlarc/static_map_inflation";
const string RosSafeCorridorName = "/tlarc/safe_corridor";
const string RosControlPoint = "/tlarc/point/contorl";
const string RosPathTopic = "/tlarc/astar/path";
const string RosTrajectoryTopic = "/tlarc/minco/trajectory";

MapLoader loader =
        MapLoader.Default
        .SetMapPath("~/Download/Tlarc/Maps/misaka/")
        .LoadMap();
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
ros.Publish<ISafeCorridor2DData<Circle2D>, MarkerArray>(
     RosSafeCorridorName, RosSafeCorridorName,
    TlarcRosBridge.Infrastructure.DataProcess.Publisher.PublishSafeCorridor);
ros.Publish<IPath2D, TlarcRosBridge.Infrastructure.Messages.Nav.Path>(
     RosPathTopic, RosPathTopic,
    TlarcRosBridge.Infrastructure.DataProcess.Publisher.PublishPath);
ros.Subscript<PointStamped, StdMessage<Vector2>>(
     RosControlPoint, RosControlPoint,
    TlarcRosBridge.Infrastructure.DataProcess.Subscriber.PointToVector2);
#endif
InflationLayerBuilder.SetPara(50);

EventBus<IGridMap2DData>.Instance.Subscribe(loader.MapEventName, x =>
{
    var infmap = InflationLayerBuilder.Build(Grid2DMap.Build_IGridMap2DData(x));
    EventBus<ISdf2D>.Instance.Publish(loader.MapEventName, infmap);
    EventBus<IGridMap2DData>.Instance.Publish(RosStaticInflationMap, infmap.GridMap.Data);
}
);

Vector2 from = new();
Vector2 to = new();
bool flag = false;
EventBus<StdMessage<Vector2>>.Instance.Subscribe(RosControlPoint, x =>
{
    if (flag)
        from = x.Instance;
    else
        to = x.Instance;
    flag = !flag;

    var astar = planner.SeachPath(from, to);
    EventBus<IPath2D>.Instance.Publish(RosPathTopic, astar);
    var safeCorridor = planner.SearchSafeCorridor(astar);
    EventBus<ISafeCorridor2DData<Circle2D>>.Instance.Publish(RosSafeCorridorName, safeCorridor);
    var trajectory = planner.OptimizePath(
        safeCorridor,
        new(safeCorridor.Corridors[0].Origin, Vector2.Zero, Vector2.Zero),
        new(safeCorridor.Corridors[^1].Origin, Vector2.Zero, Vector2.Zero));
    if (trajectory is not null) EventBus<IPath2D>.Instance.Publish(RosTrajectoryTopic,
     new PathDecorator<IEnumerable<Vector2>>(trajectory.GetPositions(trajectory.FromWhen, (trajectory.ToWhen - trajectory.FromWhen).TotalSeconds / 100f, 101), trajectory.Header));

});
loader.MapPublish();
Console.WriteLine("Map publish");
