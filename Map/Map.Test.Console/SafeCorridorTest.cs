
using System.Numerics;
using CostMap.Infrastructure.Algorithm;
using CostMap.Infrastructure.Data;
using Kernel.Core.EventBus;
using Kernel.DataInterfaces;
using Kernel.DataInterfaces.Constraints;
using Kernel.DataInterfaces.Navigation;
using Kernel.DataInterfaces.Visualization;
using Map;
using SafetyCorridor.Infractructure.CircleSafecorridor;
using SafetyCorridor.Infractructure.RectangleSafeCorridor;
using TlarcRosBridge.Infrastructure.Messages.Nav;
using TlarcRosBridge.Infrastructure.Messages.Visualization;

public static class SafeCorridorTest
{
    static readonly Vector2[] DebugPath = [
        new (2, 7),
        new (11, 5),
        new (7, -2),
        new (3, 2),
        new (14, 3),
        new (8, -5),
        new (5, -7),
        new (12, -7),
        new (1, 6),
        new (9, -2)
    ];
    public static void Build()
    {
        MapLoader loader =
                MapLoader.Default
                .SetMapPath("~/Download/Tlarc/Maps/misaka/")
                .LoadMap();

        var obs = ObstacleMap.Default.SetEventSdfMapName(loader.MapEventName).BuildMapRelatedMap();
        Console.WriteLine("Map Load");

#if true
        const string RosNodeName = "TlarcMapServer";
        const string RosStaticMap = "/tlarc/static_map";
        const string RosStaticInflationMap = "/tlarc/static_map_inflation";
        const string RosSafeCorridorName = "/tlarc/safe_corridor";

        var ros = TlarcRosBridge.Domain.RosBridge.Build(RosNodeName);

        ros.Publish<IGridMap2DData, OccupancyGrid>(
             loader.MapEventName, RosStaticMap,
            TlarcRosBridge.Infrastructure.DataProcess.Publisher.GridMap2dToOccupancyGridMap);
        ros.Publish<IGridMap2DData, OccupancyGrid>(
             RosStaticInflationMap, RosStaticInflationMap,
            TlarcRosBridge.Infrastructure.DataProcess.Publisher.GridMap2dToOccupancyGridMap);
        ros.Publish<ISafeCorridor2DData<ICircle>, MarkerArray>(
             $"{RosSafeCorridorName}_circle", $"{RosSafeCorridorName}_circle",
            TlarcRosBridge.Infrastructure.DataProcess.Publisher.PublishCircleSafeCorridor);
        ros.Publish<ISafeCorridor2DData<IRectangle>, MarkerArray>(
             $"{RosSafeCorridorName}_rectangle", $"{RosSafeCorridorName}_rectangle",
            TlarcRosBridge.Infrastructure.DataProcess.Publisher.PublishRectangleSafeCorridor);
#endif
        InflationLayerBuilder.SetPara(50);

        EventBus<IGridMap2DData>.Instance.Subscribe(loader.MapEventName, x =>
        {
            var infmap = InflationLayerBuilder.Build(Grid2DMap.Build_IGridMap2DData(x));
            EventBus<ISdf2D>.Instance.Publish(loader.MapEventName, infmap);
            EventBus<IGridMap2DData>.Instance.Publish(RosStaticInflationMap, infmap.GridMap.Data);
        }
            );

        EventBus<IObstacle>.Instance.Subscribe(obs.EventObstacleName, x =>
        {
            float dis = -1;
            var arr = (from p in DebugPath
                       where obs.Obstacle!.FindNearestObstacleDistance(p, 2, out dis)
                       select new Circle2D(dis, p) as ICircle).ToArray();
            EventBus<ISafeCorridor2DData<ICircle>>.Instance.Publish($"{RosSafeCorridorName}_circle",
                   new CircleSafecorridorImpl()
                   {
                       Header = obs.Obstacle!.Header,
                       Length = arr.Length,
                       Corridors = arr,
                   }
            );
        });

        EventBus<IObstacle>.Instance.Subscribe(obs.EventObstacleName, x =>
        {
            var arr = (from p in DebugPath
                       select new AABB2D(p.X - 0.1f, p.Y - 0.1f,
                                         p.X + 0.1f, p.Y + 0.1f) as IRectangle).ToArray();
            EventBus<ISafeCorridor2DData<IRectangle>>.Instance.Publish($"{RosSafeCorridorName}_rectangle",
                   new RectangleSafecorridorImpl()
                   {
                       Header = obs.Obstacle!.Header,
                       Length = arr.Length,
                       Corridors = arr,
                   }
            );
        });





        loader.MapPublish();
        Console.WriteLine("Map Publish");
    }
}