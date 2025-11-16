
using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using CostMap.Infrastructure.Algorithm;
using g4;
using Kernel.Contract;
using Kernel.Contract.Navigation;

namespace CostMap.Infrastructure.Data;

using System.Diagnostics;
using System.Runtime.InteropServices;
using Kernel.Utils;

[StructLayout(LayoutKind.Explicit, Size = 4)]
public struct ROGMapCell
{
    public enum StateEnum : byte
    {
        Unknow = 0,
        Occu,
        Free,
    }
    [StructLayout(LayoutKind.Explicit, Size = 1)]
    public struct State()
    {
        [FieldOffset(0)]
        private StateEnum value = StateEnum.Unknow;

        public static implicit operator bool(State s) => s.value == StateEnum.Occu;
        public static implicit operator State(bool b) => new() { value = b ? StateEnum.Occu : StateEnum.Free };
        public static implicit operator State(StateEnum b) => new() { value = b};
        // Equality operator
        public static bool operator ==(State left, StateEnum right) => left.value == right;
        public static bool operator !=(State left, StateEnum right) => left.value != right;
        
        public override readonly bool Equals(object? obj) => obj is State s && s.value == value;
        public override readonly int  GetHashCode() => value.GetHashCode();

    }
    [FieldOffset(0)] public State OccupyState;       // bool 显式为 byte
    [FieldOffset(1)] public short OccupyCount;      // 2 bytes
    [FieldOffset(3)] public sbyte OccupyDistance;   // 1 byte
}

public class ROGMap : IMap2D, IGridMap2D, IObstacle, IEnumableGridMap
{
    public readonly float TopZ;
    public readonly float ButtonZ;
    public readonly uint  Width;
    public readonly uint  Height;
    public readonly float Resolution;
    public required int   ForgetFrameCount { get; init; }
    public required float HighOccupyDensity { init { _occuDensity = value; } }
    public required float HighError { init => _highError = Math.Abs(value) / Resolution; }
    public readonly int   SizeX;
    public readonly int   SizeY;
    public readonly int   Size2D;
    public readonly int   SizeZ;

    internal readonly int   s_x_2;
    internal readonly int   s_y_2;
    internal readonly float _highError;
    internal readonly float _occuDensity;

    public required float BlindCircleRadius { get; init; }
    public required float SlidingThreshold { init { _slidingThreshold = value; } }
    internal readonly float _slidingThreshold;

    public float InflationDistance { get; }

    internal readonly float[]       _memory;
    internal readonly sbyte[]       _updateFrameCount;
    internal readonly ROGMapCell[]  _gridData = [];
    internal readonly float[]       _upper = [];
    internal readonly float[]       _lower = [];

    internal Vector2i _center;

    public float _lossHit { internal get; init; } = 0.9f;
    public float _lossMiss { internal get; init; } = -0.7f;

    /// <summary>
    ///  as p_max = 99.99%
    /// </summary>
    internal readonly float _lossMax = 4.0f;
    /// <summary>
    ///  as p_max = 99.0%
    /// </summary>
    internal readonly float _lossOccu = 2.0f;
    internal readonly float _lossFree = -2.0f;

    internal readonly int   _inflationDistance;
    internal readonly int   _inflationDistanceHalf;
    internal readonly float _inflationDistanceSquare;


    public sbyte[] UpdateFrameCount => _updateFrameCount;


    public Vector3 CenterInWorld
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get =>
        new(_center.x * Resolution, _center.y * Resolution, 0);
    }
    public Vector2 Origin { get; }


    public sbyte[] Data => GridMap.Data;

    public Header Header => GridMap.Header.Header;

    public bool Visualize = false;

    public GridMap2DData GridMap;

    public bool IsMoveAble(Vector2 from, Vector2 to)
    {
        Vector2i p1 = Algorithm.ROGMap.Index(new Vector3(from - Origin,0),this);
        Vector2i p2 = Algorithm.ROGMap.Index(new Vector3(to - Origin,0),this);
        return Geometry.BresenhamLine(p1, p2).Any(p =>
        {
            if (p.x < 0 || p.y < 0 || p.x >= SizeX || p.y >= SizeY)
                return true;
            var k = p.LocalToGlobalNormalize(this);
            return _gridData[k.x + k.y * SizeX].OccupyCount != 0;
        });
    }

    public bool IsMoveAble(Vector2 position)
    {
        Vector2i p = Algorithm.ROGMap.Index(new Vector3(position - Origin,0),this);
        if (p.x < _center.x - s_x_2 || p.y < _center.y - s_y_2 || p.x >= _center.x + s_x_2 || p.y >= _center.y + s_y_2)
            return true;
        var k = p.LocalToGlobalNormalize(this);
        return _gridData[k.x + k.y * SizeX].OccupyCount != 0;
    }


    public bool IsMoveAble(in int fromX, in int fromY, in int toX, in int toY)
    {
        return Geometry.BresenhamLine(new(fromX, fromY), new(toX, toY)).All(p =>
        {
            if (p.x < 0 || p.y < 0 || p.x >= SizeX || p.y >= SizeY)
                return true;
            var k = p.LocalToGlobalNormalize(this);
            return _gridData[k.x + k.y * SizeX].OccupyCount != 0;
        });
    }

    public bool IsMoveAble(in int positionX, in int positionY)
    {
        Vector2i p = new(positionX, positionY);
        if (p.x < 0 || p.y < 0 || p.x >= SizeX || p.y >= SizeY)
            return true;
        var k = p.LocalToGlobalNormalize(this);
        return _gridData[k.x + k.y * SizeX].OccupyCount != 0;
    }

    public bool SearchNearest(Vector2 from, float radius, out float distance)
    {
        throw new NotImplementedException();
    }

    public void All(IEnumableGridMap.StateEnum state, Action<Vector2, IEnumableGridMap.StateEnum> callback, bool paralized = false)
    {
        bool check_free(ROGMapCell gs) => state.HasFlag(IEnumableGridMap.StateEnum.Free) && gs.OccupyState == ROGMapCell.StateEnum.Free;
        bool check_unknow(ROGMapCell gs) => state.HasFlag(IEnumableGridMap.StateEnum.Unknow) && gs.OccupyState == ROGMapCell.StateEnum.Unknow;
        bool check_occu(ROGMapCell gs) => state.HasFlag(IEnumableGridMap.StateEnum.Occu) && gs.OccupyCount != 0;
        
        void process(int x1,int y1)
        {
            var c = new Vector2i(x1, y1).LocalToGlobalNormalize(this);
            var x = c.x;
            var y = c.y;
            ref var g = ref _gridData[x + y * SizeX];
            Vector2 pos() => new Vector2(x1,y1) * Resolution + Origin;
            if(check_free(g)) callback(pos(), IEnumableGridMap.StateEnum.Free);
            if(check_occu(g)) callback(pos(), IEnumableGridMap.StateEnum.Occu);
            if(check_unknow(g)) callback(pos(), IEnumableGridMap.StateEnum.Unknow);
        }
        if (paralized)
        {
            BlockParallel.For(SizeX, SizeY, 0, 0, process);
            return;
        }
        for(int i = 0;i < SizeX; ++i)
        for(int j = 0;j < SizeY; ++j)
            process(i,j);
    }

    public ROGMap(uint height, uint width, float inflationDistance, float resolution, float topZ, float buttonZ,string identifier)
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
        _inflationDistanceSquare = _inflationDistanceHalf * _inflationDistanceHalf;
        _gridData = new ROGMapCell[SizeX * SizeY];
        _memory = new float[SizeX * SizeY * SizeZ];
        _upper = new float[SizeX * SizeY];
        _lower = new float[SizeX * SizeY];
        Array.Fill(_upper, -1e6f);
        Array.Fill(_lower, 1e6f);
        _updateFrameCount = new sbyte[SizeX * SizeY * SizeZ];
        
        GridMap = new GridMap2DData()
        {
            Data            = new sbyte[SizeX * SizeY],
            Header          = new(){
                Header = new(){Identifier = identifier},
                Height          = Height,
                Width           = Width,
                Origin          = Origin,
                Resolution      = resolution,
                RotationMatrix  = Matrix3x2.Identity,
                RotationRad     = 0
            }
        };
    }
}
