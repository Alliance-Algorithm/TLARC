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

    public required HeaderInner HeaderData { get; set; }
    public required Vector2 Origin { get; set; }
    public required uint Width { get; set; }
    public required uint Height { get; set; }
    public required double RotationRad { get; set; }
    public required Matrix3x2 RotationMatrix { get; set; }
    public required float Resolution { get; set; }
    public required sbyte[] Data { get; set; }
}