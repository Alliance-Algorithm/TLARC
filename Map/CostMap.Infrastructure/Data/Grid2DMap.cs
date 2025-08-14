using System.Numerics;
using CostMap.Infrastructure.Algorithm;
using Kernel.DataInterfaces;
using Kernel.DataInterfaces.Navigation;

namespace CostMap.Infrastructure.Data;

public class Grid2DMap : IMap2D, IGridMap2D
{
    public required Grid2DMapData DataChangeable { get; init; }
    public IGridMap2DData Data => DataChangeable;
    public IMap2D Actions => this;

    public bool IsMoveAble(Vector2 from, Vector2 to) =>
        GridMapInner.CheckMoveable(from, to, Data, 50, GridMapInner.ThresholdType.LessEqual);

    public bool IsMoveAble(Vector2 position) =>
        GridMapInner.CheckMoveable(position, Data, 50, GridMapInner.ThresholdType.LessEqual);

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
}