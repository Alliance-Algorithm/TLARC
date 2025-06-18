using System.Numerics;
using CostMap.Infrastructure.Algorithm;
using Kernel.DataInterfaces.Navigation;

namespace CostMap.Infrastructure.Data;

public class OccupancyGrid2DMap : IOccupancyGridMap2D
{
    public required string Identifier { get; init; }
    public required Vector2 Origin { get; init; }
    public required uint Width { get; init; }
    public required uint Height { get; init; }
    public required double RotationRad { get; init; }
    public required Matrix3x2 RotationMatrix { get; init; }
    public required float Resolution { get; init; }
    public required sbyte[] Data { get; init; }
    public required sbyte Threshold { get; init; }

    public required float[] OccupancyRate { get; init; }


    public bool IsMoveAble(Vector2 from, Vector2 to)
    {
        return GridMapInner.CheckMoveable(from, to, this, Threshold, GridMapInner.ThresholdType.LessEqual);
    }

    public bool IsMoveAble(Vector2 position)
    {
        return GridMapInner.CheckMoveable(position, this, Threshold, GridMapInner.ThresholdType.LessEqual);
    }

    private OccupancyGrid2DMap() { }

    public static OccupancyGrid2DMap Build(IOccupancyGridMap2D data)
    {
        return new OccupancyGrid2DMap
        {
            Identifier = data.Identifier,
            Origin = data.Origin,
            Height = data.Height,
            Width = data.Width,
            RotationRad = data.RotationRad,
            RotationMatrix = data.RotationMatrix,
            Resolution = data.Resolution,
            Data = data.Data,
            Threshold = data.Threshold,
            OccupancyRate = new float[data.Data.Length]
        };
    }
}