using CostMap.Infrastructure.Algorithm;
using g4;
using Kernel.DataInterfaces.Navigation;

namespace CostMap.Infrastructure.Data;

public class OccupancyGrid2DMap : IOccupancyGridMap2D
{
    public required Vector2d Origin         { get; init; }
    public required Vector2i Size           { get; init; }
    public required double   RotationRad    { get; init; }
    public required Matrix2d RotationMatrix { get; init; }
    public required double   Resolution     { get; init; }
    public required sbyte[]  Data           { get; init; }
    public required sbyte    Threshold      { get; init; }


    public bool IsMoveAble(Vector2d from, Vector2d to)
    {
        return GridMapInner.CheckMoveable(from, to, this, Threshold, GridMapInner.ThresholdType.LessEqual);
    }

    public bool IsMoveAble(Vector2d position)
    {
        return GridMapInner.CheckMoveable(position, this, Threshold, GridMapInner.ThresholdType.LessEqual);
    }

    private OccupancyGrid2DMap() { }

    public static OccupancyGrid2DMap Build(IOccupancyGridMap2D data)
    {
        return new OccupancyGrid2DMap
        {
            Origin         = data.Origin,
            Size           = data.Size,
            RotationRad    = data.RotationRad,
            RotationMatrix = data.RotationMatrix,
            Resolution     = data.Resolution,
            Data           = data.Data,
            Threshold      = data.Threshold
        };
    }
}
