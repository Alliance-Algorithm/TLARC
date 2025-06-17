using TlarcRosBridge.Infrastructure.Decorators;
using g4;
using Kernel.Core.EventBus;
using Kernel.DataInterfaces.Navigation;
using TlarcRosBridge.Infrastructure.Messages.Nav;

namespace TlarcRosBridge.Test;

[TestFixture]
public class MapTest
{
    private class GridMap2DData : IGridMap2DData
    {
        public required Vector2i Size { get; init; }
        public required sbyte[] Data { get; init; }

        public double RotationRad { get; init; } = 0;
        public Matrix2d RotationMatrix { get; init; } = Matrix2d.Identity;
        public Vector2d Origin { get; init; } = new();
        public double Resolution { get; init; } = 0.1;
    }

    private IGridMap2DData MapGenerator(int width, int height)
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
            Size = new Vector2i(width, height),
            Data = data
        };
    }

    [SetUp]
    public void Setup()
    {
        var ros = Domain.RosBridge.Build("map_test");
        ros.Publish<IGridMap2DData, OccupancyGrid>("ros_test_map", "ros_test_map",
            Domain.DataProcess.Publisher.GridMap2dToOccupancyGridMap);
    }

    [Test]
    public void Test1()
    {
        EventBus.Instance.Publish("ros_test_map", MapGenerator(1000, 500));
        Thread.Sleep(1000);
    }
}