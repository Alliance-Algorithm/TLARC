using System.Numerics;
using Kernel.Core.EventBus;
using Kernel.Contract;
using Kernel.Contract.Navigation;
using TlarcRosBridge.Infrastructure.Messages.Nav;

namespace TlarcRosBridge.Test;

[TestFixture]
public class MapTest
{
    private GridMap2DData MapGenerator(uint width, uint height)
    {
        var data = new sbyte[width * height];
        for (var i = 0; i < width; i++)
            for (var j = 0; j < height; j++)
            {
                var xr = (i * 4.0 - 2 * width) / width;
                var yr = (j * 4.0 - 2 * height) / height;
                var val = xr * xr + double.Pow(yr - double.Pow(xr, 2 / 3.0), 2);
                if (val <= 1 || xr < 0)
                    data[i + width * j] = (sbyte)(50 + xr * 10 + yr * 10);
            }

        return new GridMap2DData
        {
            Header = new()
            {
                Width = width,
                Height = height
            },
            Data = data
        };
    }

    [SetUp]
    public void Setup()
    {
        var ros = Domain.RosBridge.Build("map_test");
        ros.Publish<GridMap2DData, OccupancyGrid>("ros_test_map", "ros_test_map",
            Infrastructure.DataProcess.Publisher.GridMap2dToOccupancyGridMap);
    }

    [Test]
    public void Test1()
    {
        EventBus<GridMap2DData>.Instance.Publish("ros_test_map", MapGenerator(1000, 500));
        Thread.Sleep(1000);
        Environment.Exit(0);
    }
}