using System.Numerics;
using CostMap.Infrastructure.Algorithm;
using Kernel.DataInterfaces;
using Kernel.DataInterfaces.Navigation;

namespace CostMap.Infrastructure.Data;

public class Grid2DMap : IMap2D, IGridMap2D
{
    public class DataInner : IGridMap2DData
    {
        public class HeaderInner(string id) : IHeader
        {
            public string Identifier { get; set; } = id;
        }

        public IHeader Header => HeaderData;

        public required HeaderInner HeaderData { get; init; }
        public required Vector2 Origin { get; init; }
        public required uint Width { get; init; }
        public required uint Height { get; init; }
        public required double RotationRad { get; init; }
        public required Matrix3x2 RotationMatrix { get; init; }
        public required float Resolution { get; init; }
        public required sbyte[] Data { get; init; }
    }

    public DataInner DataChangable { get; init; }
    public IGridMap2DData Data => DataChangable;
    public IMap2D Actions => this;

    public bool IsMoveAble(Vector2 from, Vector2 to) =>
        GridMapInner.CheckMoveable(from, to, Data, 50, GridMapInner.ThresholdType.LessEqual);

    public bool IsMoveAble(Vector2 position) =>
        GridMapInner.CheckMoveable(position, Data, 50, GridMapInner.ThresholdType.LessEqual);

    private Grid2DMap() { }


    public static Grid2DMap Build_IGridMap2DData(IGridMap2DData data) =>
        new()
        {
            DataChangable = new DataInner
            {
                HeaderData = new DataInner.HeaderInner(data.Header.Identifier),
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