using System.Numerics;
using System.Runtime.CompilerServices;
using CostMap.Infrastructure.Algorithm;
using Kernel.DataInterfaces;
using Kernel.DataInterfaces.Navigation;

namespace CostMap.Infrastructure.Data;

public class InflationMap : IMap2D, ISdf2D
{

    public required Grid2DMap GridMap { get; init; }
    public readonly float _distance;

    public IHeader Header => GridMap.Header;

    public bool IsMoveAble(Vector2 from, Vector2 to) => GridMap.IsMoveAble(from, to);

    public bool IsMoveAble(Vector2 position) => GridMap.IsMoveAble(position);

    public bool IsMoveAble(Vector2 point, out float distance)
    {
        distance = -1;
        if (IsMoveAble(point)) return true;
        var p = point - GridMap.Data.Origin;
        distance = (100 - GridMap.DataChangeable.Data[
           (int)Math.Round(p.X / GridMap.Data.Resolution) +
            (int)Math.Round(p.Y / GridMap.Data.Resolution) * GridMap.Data.Width]) / 100.0f * _distance;
        return false;
    }

    private InflationMap(float distance)
    {
        _distance = distance;
    }

    public static InflationMap New_IGridMap2DData(in IGridMap2DData data, in float distance) =>
    new(distance) { GridMap = Grid2DMap.New_IGridMap2DData(data) };
}