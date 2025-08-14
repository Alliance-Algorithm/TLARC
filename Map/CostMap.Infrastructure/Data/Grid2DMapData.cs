using System.Numerics;
using CostMap.Infrastructure.Algorithm;
using Kernel.DataInterfaces;
using Kernel.DataInterfaces.Navigation;

namespace CostMap.Infrastructure.Data;

public class Grid2DMapData : IGridMap2DData
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