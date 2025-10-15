
using System.Numerics;
using CostMap.Infrastructure.Algorithm;
using CostMap.Infrastructure.Data;
using Kernel.Core.EventBus;
using Kernel.Contract;
using Kernel.Contract.Constraints;
using Kernel.Contract.Navigation;
using Kernel.Contract.Visualization;
using Map;
using SafetyCorridor.Infractructure.CircleSafecorridor;
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

        ros.Publish<GridMap2DData, OccupancyGrid>(
             loader.MapEventName, RosStaticMap,
            TlarcRosBridge.Infrastructure.DataProcess.Publisher.GridMap2dToOccupancyGridMap);
        ros.Publish<GridMap2DData, OccupancyGrid>(
             RosStaticInflationMap, RosStaticInflationMap,
            TlarcRosBridge.Infrastructure.DataProcess.Publisher.GridMap2dToOccupancyGridMap);
        ros.Publish<SafeCorridor2DData<Circle>, MarkerArray>(
             $"{RosSafeCorridorName}_circle", $"{RosSafeCorridorName}_circle",
            TlarcRosBridge.Infrastructure.DataProcess.Publisher.PublishCircleSafeCorridor);
        ros.Publish<SafeCorridor2DData<Rectangle>, MarkerArray>(
             $"{RosSafeCorridorName}_rectangle", $"{RosSafeCorridorName}_rectangle",
            TlarcRosBridge.Infrastructure.DataProcess.Publisher.PublishRectangleSafeCorridor);
#endif
        InflationLayerBuilder.SetPara(50);

        EventBus<GridMap2DData>.Instance.Subscribe(loader.MapEventName, x =>
        {
            var map = Grid2DMap.Build_GridMap2DData(x);
            var infmap = InflationLayerBuilder.Build(map,map.Data);
            EventBus<ISdf2D>.Instance.Publish(loader.MapEventName, infmap);
            EventBus<GridMap2DData>.Instance.Publish(RosStaticInflationMap, infmap.GridMap.Data);
        }
            );

        EventBus<IObstacle>.Instance.Subscribe(obs.EventObstacleName, x =>
        {
            float dis = -1;
            var arr = (from p in DebugPath
                       where obs.Obstacle!.FindNearestObstacleDistance(p, 2, out dis)
                       select new Circle(){R = dis,Origin = p}).ToArray();
            EventBus<SafeCorridor2DData<Circle>>.Instance.Publish($"{RosSafeCorridorName}_circle",
                   new SafeCorridor2DData<Circle>()
                   {
                       Length = arr.Length,
                       Corridors = arr,
                   }
            );
        });

        EventBus<IObstacle>.Instance.Subscribe(obs.EventObstacleName, x =>
        {
            var arr = (from p in DebugPath
                       select new AABB2D(){
                                        MinX = p.X - 0.1f, 
                                        MinY = p.Y - 0.1f,
                                        MaxX = p.X + 0.1f, 
                                        MaxY = p.Y + 0.1f}).ToArray();
        });





        loader.MapPublish();
        Console.WriteLine("Map Publish");
    }
}