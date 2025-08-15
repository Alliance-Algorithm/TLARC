
using System.Buffers;
using System.Numerics;
using g4;
using Kernel.DataInterfaces;
using Kernel.DataInterfaces.Navigation;

namespace CostMap.Infrastructure.Data;

public class ROGMap : IMap2D, IHeader, IGridMap2DData
{

    public int CenterX => _center.x;
    public int CenterY => _center.y;
    public required uint Width { get; init { var v = value - value % 2 + 1; field = v; SizeX = (int)v; } }
    public required uint Height { get; init { var v = value - value % 2 + 1; field = v; SizeY = (int)v; } }
    public required float TopZ { get; init; }
    public required float ButtonZ { get; init; }
    public required float Resolution { get; init; }
    public required int ForgetFrameCount { get; init; }

    public readonly int SizeX;
    public readonly int SizeY;


    public required float BlindCircleRadius { get; init; }
    public required float SlidingThreshold { get; init; }


    public float InflationDistance { private get; init; }

    internal readonly float[] _memory;
    internal readonly sbyte[] _updateFrameCount;
    internal uint[] _gridData = [];
    internal float[] _upper = [];
    internal float[] _lower = [];

    internal Vector2i _center;

    public float _lossHit { internal get; init; } = 0.9f;
    public float _lossMiss { internal get; init; } = -0.7f;
    public required string Identifier { get; init; }

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

    public float[] Memory => _memory;

    public sbyte[] UpdateFrameCount => _updateFrameCount;
    public IMap2D Actions => this;

    public IHeader Header => this;

    public Vector2 Origin => new((CenterX - SizeX / 2) * Resolution, (CenterY - SizeY / 2) * Resolution);

    public double RotationRad => 0;

    public Matrix3x2 RotationMatrix => Matrix3x2.Identity;

    internal readonly sbyte[] _data;
    public sbyte[] Data => _data;

    public bool IsMoveAble(Vector2 from, Vector2 to)
    {
        throw new NotImplementedException();
    }

    public bool IsMoveAble(Vector2 position)
    {
        throw new NotImplementedException();
    }
    public ROGMap()
    {
        _inflationDistance = (int)Math.Round(InflationDistance / Resolution);
        _inflationDistanceHalf = _inflationDistance / 2;
        _gridData = new uint[SizeX * SizeY];
        _memory = new float[SizeX * SizeY];
        _upper = new float[SizeX * SizeY];
        _lower = new float[SizeX * SizeY];
        _updateFrameCount = new sbyte[SizeX * SizeY];
        _data = new sbyte[SizeX * SizeY];
    }


}
