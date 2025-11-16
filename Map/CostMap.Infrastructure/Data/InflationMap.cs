using System.Numerics;
using System.Runtime.CompilerServices;
using CostMap.Infrastructure.Algorithm;
using Kernel.Contract;
using Kernel.Contract.Navigation;

namespace CostMap.Infrastructure.Data;

public class InflationMap : IMap2D, IGridMap2D
{

    public required Grid2DMap GridMap { get; init; }
    public readonly float _distance;

    public Header Header => GridMap.Header;

    public Vector2 Origin => GridMap.Origin;

    public bool IsMoveAble(Vector2 from, Vector2 to) => GridMap.IsMoveAble(from, to);

    public bool IsMoveAble(Vector2 position) => GridMap.IsMoveAble(position);

    public bool IsMoveAble(Vector2 point, out float distance)
    {
        distance = -1;
        if (IsMoveAble(point)) return true;
        var p = (point - GridMap.Data.Header.Origin) / GridMap.Data.Header.Resolution;
        distance = (100 - GridMap.Data.Data[
           (int)p.X +
            (int)p.Y * GridMap.Data.Header.Width]) / 100.0f * _distance;
        return false;
    }

    private InflationMap(float distance)
    {
        _distance = distance;
    }

    public static InflationMap New_GridMap2DData(in GridMap2DData data, in float distance) =>
    new(distance) { GridMap = Grid2DMap.New_GridMap2DData(data) };

    public bool IsMoveAble(in int fromX, in int fromY, in int toX, in int toY)
    {
        throw new NotImplementedException();
    }

    public bool IsMoveAble(in int positionX, in int positionY)
    {
        throw new NotImplementedException();
    }
}