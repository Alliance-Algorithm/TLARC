
using Kernel.Contract.Navigation;
using Map;
using TlarcRosBridge.Infrastructure.Messages.Nav;

public static class StaticMapTest
{
        public static void Build()
        {
                MapLoader loader =
                        MapLoader.Default
                        .SetMapPath("~/Download/Tlarc/Maps/TestHigh/")
                        .LoadMap();
                Console.WriteLine("Map Load");

#if true
                const string RosNodeName = "TlarcMapServer";
                const string RosStaticMap = "/tlarc/static_map";

                var ros = TlarcRosBridge.Domain.RosBridge.Build(RosNodeName);

                ros.Publish<GridMap2DData, OccupancyGrid>(
                     loader.MapEventName, RosStaticMap,
                    TlarcRosBridge.Infrastructure.DataProcess.Publisher.GridMap2dToOccupancyGridMap);
#endif


                loader.MapPublish();
                Console.WriteLine("Map Publish");
        }
}