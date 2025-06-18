using CostMap.Infrastructure.Data;
using g4;
using Kernel.DataInterfaces.Navigation;
using Kernel.DataInterfaces.Sensor;

namespace CostMap.Infrastructure.Algorithm;

public static class OccupancyGridMapBuilder
{
    public static void UpdateFromPoint(in Vector3f point, ref OccupancyGrid2DMap map)
    {
        var data = map.Data;
    }
}