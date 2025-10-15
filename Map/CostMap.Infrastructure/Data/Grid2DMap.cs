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


    public Header Header => DataChangeable.Header;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsMoveAble(Vector2 from, Vector2 to) =>
            GridMapInner.CheckMoveable(from, to, Data, 100, GridMapInner.ThresholdType.GreaterEqual);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsMoveAble(Vector2 position) =>
        GridMapInner.CheckMoveable(position, Data, 100, GridMapInner.ThresholdType.GreaterEqual);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsMoveAble(in int fromX, in int fromY, in int toX, in int toY) =>
            IsMoveAble(new Vector2(fromX, fromY) * Data.Resolution + Data.Origin, new Vector2(toX, toY) * Data.Resolution + Data.Origin);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsMoveAble(in int positionX, in int positionY) =>
            IsMoveAble(new Vector2(positionX, positionY) * Data.Resolution + Data.Origin);

    private Grid2DMap() { }


    public static Grid2DMap Build_GridMap2DData(GridMap2DData data) =>
        new()
        {
            DataChangeable = new GridMap2DData
            {
                Header = new Header{ Identifier = data.Header.Identifier},
                Origin = data.Origin,
                Height = data.Height,
                Width = data.Width,
                RotationRad = data.RotationRad,
                RotationMatrix = data.RotationMatrix,
                Resolution = data.Resolution,
                Data = data.Data
            }
        };
    public static Grid2DMap New_GridMap2DData(GridMap2DData data) =>
    new()
    {
        DataChangeable = new GridMap2DData
        {
            Header = new Header{ Identifier = data.Header.Identifier},
            Origin = data.Origin,
            Height = data.Height,
            Width = data.Width,
            RotationRad = data.RotationRad,
            RotationMatrix = data.RotationMatrix,
            Resolution = data.Resolution,
            Data = new sbyte[data.Width * data.Height]
        }
    };

}