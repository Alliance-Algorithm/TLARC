
using System.Buffers;
using System.Numerics;
using g4;
using Kernel.DataInterfaces.Navigation;

namespace CostMap.Infrastructure.Data;

public class ROGMap : IMap2D, IOccupancyGridMap2D
{

    public int CenterX => _center.x;
    public int CenterY => _center.y;
    public required uint Width { get; init; }
    public required uint Height { get; init; }
    public required float TopZ { get; init; }
    public required float ButtonZ { get; init; }
    public required float Resolution { get; init; }
    public required int SizeX { get; init; }
    public required int SizeY { get; init; }
    public required int ForgetFrameCount { get; init; }

    public required float BlindCircleRadius { get; init; }
    public required int SlidingThreshold { get; init; }


    internal float InflationDistance { get; init; }

    internal float[]? _memory;
    internal float[]? _updateFrameCount;

    internal uint[] _gridData;

    internal float[] _upper = [];
    internal float[] _lower = [];

    internal Vector2i _center;

    internal float _lossHit { get; init; } = 0.9f;
    internal float _lossMiss { get; init; } = -0.7f;

    /// <summary>
    ///  as p_max = 99.9999%
    /// </summary>
    internal readonly float _lossMax = 6.0f;
    /// <summary>
    ///  as p_max = 99.0%
    /// </summary>
    internal readonly float _lossOccu = 2.0f;
    internal readonly float _lossFree = -2.0f;

    internal readonly int _inflationDistance;
    internal readonly int _inflationDistanceHalf;

    private OccupancyGridData? _data;

    public float[] Memory => _memory ?? throw new NullReferenceException("ROGMap does not build, run ROGMap.Build() first");

    public float[] UpdateFrameCount => _updateFrameCount ?? throw new NullReferenceException("ROGMap does not build, run ROGMap.Build() first");

    public IMap2D Actions => this;

    public IOccupancyGridMap2DData OccupancyData => _data ?? throw new NullReferenceException("ROGMap does not build, run ROGMap.Build() first");

    public bool IsMoveAble(Vector2 from, Vector2 to)
    {
        throw new NotImplementedException();
    }

    public bool IsMoveAble(Vector2 position)
    {
        throw new NotImplementedException();
    }
    internal ROGMap()
    {

    }

    public ROGMap Build()
    {
        return this;
    }

}
