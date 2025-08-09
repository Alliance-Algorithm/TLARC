using System.Numerics;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using Kernel.Core.EventBus;
using Kernel.DataInterfaces;
using Kernel.DataInterfaces.Navigation;

namespace Kernel.Benchmarks;

[MemoryDiagnoser]
public class EventBusBenchmarks
{
    private const string LowEvent = "LowEvent";
    private const string HighEvent = "HighEvent";
    private const string HeavyEvent = "HeavyEvent";
    private const string HighHeavyEvent = "nodeC";
    private const string CrossEvent = "CrossEvent";


    public class OccupancyGrid2DMap : IGridMap2DData
    {
        public struct HeaderInner(string id) : IHeader
        {
            public string Identifier { get; set; } = id;
        }

        public required IHeader Header { get; init; } 
        public required Vector2 Origin { get; init; }

        public required uint Width { get; init; }
        public required uint Height { get; init; }
        public required double RotationRad { get; init; }
        public required Matrix3x2 RotationMatrix { get; init; }
        public required float Resolution { get; init; }
        public required sbyte[] Data { get; init; }

        public OccupancyGrid2DMap() { }
    }

    private OccupancyGrid2DMap _data = new()
    {
        Header = new OccupancyGrid2DMap.HeaderInner(""),
        Height = 1000,
        Width = 1000,
        Origin = new Vector2(),
        RotationMatrix = Matrix3x2.Identity,
        Resolution = 1,
        Data = new sbyte [1000 * 1000],
        RotationRad = 0
    };

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Process() { }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Process(IGridMap2DData data) { }

    [GlobalSetup]
    public void GlobalSetup()
    {
        for (var i = 0; i < 5; i++)
            EventBus.Instance.Subscribe(LowEvent, Process);
        for (var i = 0; i < 20; i++)
            EventBus.Instance.Subscribe(HighEvent, Process);

        for (var i = 0; i < 5; i++)
            EventBus<IGridMap2DData>.Instance.Subscribe(HeavyEvent, Process);
        for (var i = 0; i < 20; i++)
            EventBus<IGridMap2DData>.Instance.Subscribe(HighHeavyEvent, Process);
        for (var i = 0; i < 5; i++)
        {
            EventBus.Instance.Subscribe(CrossEvent, Process);
            EventBus<IGridMap2DData>.Instance.Subscribe(CrossEvent, Process);
        }
    }


    [Benchmark]
    public void LowEvent_Test() => EventBus.Instance.Publish(LowEvent);


    [Benchmark]
    public void HighEvent_Test() => EventBus.Instance.Publish(HighEvent);

    [Benchmark]
    public void HeavyEvent_Test() => EventBus<IGridMap2DData>.Instance.Publish(HeavyEvent, _data);

    [Benchmark]
    public void HighHeavyEvent_Test() => EventBus<IGridMap2DData>.Instance.Publish(HighHeavyEvent, _data);

    [Benchmark]
    public void CrossEvent_Test1() => EventBus.Instance.Publish(CrossEvent);

    [Benchmark]
    public void CrossEvent_Test2() => EventBus<IGridMap2DData>.Instance.Publish(CrossEvent, _data);
}