using System.Numerics;
using CostMap.Infrastructure.Algorithm;
using Kernel.DataInterfaces.Constraints;
using Kernel.DataInterfaces.Navigation;

namespace CostMap.Infrastructure.Data;


public class OccupancyGridData : IOccupancyGridMap2DData
{
    public IGridMap2DData GridMapData => DataChangeable;
    public required Grid2DMapData DataChangeable { get; init; }
    public sbyte Threshold { get; init; } = 70;
    public float LossFree { get; init; } = 0.7f;
    public float LossOccu { get; init; } = -1.9f;
    public required float[] OccupancyRate { get; set; }
}
