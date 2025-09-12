using System.Numerics;
using System.Runtime.CompilerServices;
using CostMap.Infrastructure.Algorithm;
using Kernel.DataInterfaces;
using Kernel.DataInterfaces.Navigation;

namespace CostMap.Infrastructure.Data;

public class Grid2DMap : IMap2D, IGridMap2D
{
    public required Grid2DMapData DataChangeable { get; init; }
    public IGridMap2DData Data => DataChangeable;
    public IMap2D Actions => this;


    public IHeader Header => DataChangeable.Header;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsMoveAble(Vector2 from, Vector2 to) =>
            GridMapInner.CheckMoveable(from, to, Data, 50, GridMapInner.ThresholdType.LessEqual);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsMoveAble(Vector2 position) =>
        GridMapInner.CheckMoveable(position, Data, 50, GridMapInner.ThresholdType.LessEqual);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsMoveAble(in int fromX, in int fromY, in int toX, in int toY) =>
            IsMoveAble(new Vector2(fromX, fromY) * Data.Resolution + Data.Origin, new Vector2(toX, toY) * Data.Resolution + Data.Origin);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsMoveAble(in int positionX, in int positionY) =>
            IsMoveAble(new Vector2(positionX, positionY) * Data.Resolution + Data.Origin);

    private Grid2DMap() { }


    public static Grid2DMap Build_IGridMap2DData(IGridMap2DData data) =>
        new()
        {
            DataChangeable = new Grid2DMapData
            {
                HeaderData = new Grid2DMapData.HeaderInner(data.Header.Identifier),
                Origin = data.Origin,
                Height = data.Height,
                Width = data.Width,
                RotationRad = data.RotationRad,
                RotationMatrix = data.RotationMatrix,
                Resolution = data.Resolution,
                Data = data.Data
            }
        };
    public static Grid2DMap New_IGridMap2DData(IGridMap2DData data) =>
    new()
    {
        DataChangeable = new Grid2DMapData
        {
            HeaderData = new Grid2DMapData.HeaderInner(data.Header.Identifier),
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