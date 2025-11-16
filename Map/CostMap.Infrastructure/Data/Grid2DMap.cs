using System.Numerics;
using System.Runtime.CompilerServices;
using CostMap.Infrastructure.Algorithm;
using Kernel.Contract;
using Kernel.Contract.Navigation;

namespace CostMap.Infrastructure.Data;

public class Grid2DMap : IMap2D, IGridMap2D
{
    public required GridMap2DData DataChangeable { get; init; }
    public GridMap2DData Data => DataChangeable;
    public IMap2D Actions => this;


    public Header Header => DataChangeable.Header.Header;

    public Vector2 Origin => Data.Header.Origin;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsMoveAble(Vector2 from, Vector2 to) =>
            GridMapInner.CheckMoveable(from, to, Data, 100, GridMapInner.ThresholdType.GreaterEqual);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsMoveAble(Vector2 position) =>
        GridMapInner.CheckMoveable(position, Data, 100, GridMapInner.ThresholdType.GreaterEqual);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsMoveAble(in int fromX, in int fromY, in int toX, in int toY) =>
            IsMoveAble(new Vector2(fromX, fromY) * Data.Header.Resolution + Data.Header.Origin, new Vector2(toX, toY) * Data.Header.Resolution + Data.Header.Origin);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsMoveAble(in int positionX, in int positionY) =>
            IsMoveAble(new Vector2(positionX, positionY) * Data.Header.Resolution + Data.Header.Origin);

    private Grid2DMap() { }


    public static Grid2DMap Build_GridMap2DData(GridMap2DData data) =>
        new()
        {
            DataChangeable = new GridMap2DData
            {
                Header = data.Header,
                Data = data.Data
            }
        };
    public static Grid2DMap New_GridMap2DData(GridMap2DData data) =>
    new()
    {
        DataChangeable = new GridMap2DData
        {
            Header = data.Header,
            Data = new sbyte[data.Header.Width * data.Header.Height]
        }
    };

}