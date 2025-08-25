
using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using CostMap.Infrastructure.Algorithm;
using g4;
using Kernel.DataInterfaces;
using Kernel.DataInterfaces.Navigation;

namespace CostMap.Infrastructure.Data;

public class ROGMap : IMap2D, IHeader, IGridMap2DData, IGridMap2D
{

    public int CenterX => _center.x;
    public int CenterY => _center.y;
    public uint Width { get; }
    public uint Height { get; }

    public float TopZ { get; private set; }
    public float ButtonZ { get; private set; }
    public float Resolution { get; }
    public required int ForgetFrameCount { get; init; }
    public required float HighOccupyDensity { init { _occuDensity = value; } }
    public required float HighError { init => _highError = Math.Abs(value) / Resolution; }
    public readonly int SizeX;
    public readonly int SizeY;
    public readonly int Size2D;
    public readonly int SizeZ;

    internal readonly int s_x_2;
    internal readonly int s_y_2;
    internal readonly float _highError;
    internal readonly float _occuDensity;

    public required float BlindCircleRadius { get; init; }
    public required float SlidingThreshold { init { _slidingThreshold = value; } }
    internal readonly float _slidingThreshold;

    public float InflationDistance { get; }

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
    ///  as p_max = 99.99%
    /// </summary>
    internal readonly float _lossMax = 4.0f;
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

    public Vector3 CenterInWorld
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get =>
        new((CenterX) * Resolution, (CenterY) * Resolution, 0);
    }
    public Vector2 Origin { get; }

    public double RotationRad => 0;

    public Matrix3x2 RotationMatrix => Matrix3x2.Identity;

    internal readonly sbyte[] _data;
    public sbyte[] Data => _data;

    IGridMap2DData IGridMap2D.Data => this;

    public bool IsMoveAble(Vector2 from, Vector2 to)
    {
        throw new NotImplementedException();
    }

    public bool IsMoveAble(Vector2 position)
    {
        throw new NotImplementedException();
    }


    public bool IsMoveAble(in int fromX, in int fromY, in int toX, in int toY)
    {
        return Geometry.BresenhamLine(new(fromX, fromY), new(toX, toY)).AsParallel().All(p =>
        {
            if (p.x < 0 || p.y < 0 || p.x >= SizeX || p.y >= SizeY)
                return true;
            var k = p.LocalToGlobalNormalize(this);
            return _gridData[k.x + k.y * SizeX] == 0;
        });
    }

    public bool IsMoveAble(in int positionX, in int positionY)
    {
        Vector2i p = new(positionX, positionY);
        if (p.x < 0 || p.y < 0 || p.x >= SizeX || p.y >= SizeY)
            return true;
        var k = p.LocalToGlobalNormalize(this);
        return _gridData[k.x + k.y * SizeX] == 0;
    }


    public ROGMap(uint height, uint width, float inflationDistance, float resolution, float topZ, float buttonZ)
    {
        TopZ = topZ;
        ButtonZ = buttonZ;
        SizeZ = (int)Math.Round((TopZ - ButtonZ) / resolution);

        var v = height - height % 2 + 1;
        Height = v; SizeX = (int)v;
        v = width - width % 2 + 1;
        Width = v; SizeY = (int)v;
        InflationDistance = inflationDistance;
        Resolution = resolution;
        Size2D = SizeX * SizeY;

        s_x_2 = SizeX / 2;
        s_y_2 = SizeY / 2;

        Origin = new(-height / 2 * resolution, -width / 2 * resolution);

        _inflationDistance = (int)Math.Round(InflationDistance / Resolution);
        _inflationDistance = _inflationDistance * 2 + 1;
        _inflationDistanceHalf = _inflationDistance / 2;
        _gridData = new uint[SizeX * SizeY];
        _memory = new float[SizeX * SizeY * SizeZ];
        _upper = new float[SizeX * SizeY];
        _lower = new float[SizeX * SizeY];
        Array.Fill(_upper, -1e6f);
        Array.Fill(_lower, 1e6f);
        _updateFrameCount = new sbyte[SizeX * SizeY];
        _data = new sbyte[SizeX * SizeY];
    }


}
