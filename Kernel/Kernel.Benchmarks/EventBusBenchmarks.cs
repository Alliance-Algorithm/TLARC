using System.Numerics;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using Kernel.Core.EventBus;
using Kernel.Contract;
using Kernel.Contract.Navigation;

namespace Kernel.Benchmarks;

[MemoryDiagnoser]
public class EventBusBenchmarks
{
    private const string LowEvent = "LowEvent";
    private const string HighEvent = "HighEvent";
    private const string HeavyEvent = "HeavyEvent";
    private const string HighHeavyEvent = "nodeC";
    private const string CrossEvent = "CrossEvent";


    private GridMap2DData _data = new()
    {
        Header = new Header{Identifier = ""},
        Height = 1000,
        Width = 1000,
        Origin = new Vector2(),
        RotationMatrix = Matrix3x2.Identity,
        Resolution = 1,
        Data = new sbyte[1000 * 1000],
        RotationRad = 0
    };

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Process() { }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Process(GridMap2DData data) { }

    [GlobalSetup]
    public void GlobalSetup()
    {
        for (var i = 0; i < 5; i++)
            EventBus.Instance.Subscribe(LowEvent, Process);
        for (var i = 0; i < 20; i++)
            EventBus.Instance.Subscribe(HighEvent, Process);

        for (var i = 0; i < 5; i++)
            EventBus<GridMap2DData>.Instance.Subscribe(HeavyEvent, Process);
        for (var i = 0; i < 20; i++)
            EventBus<GridMap2DData>.Instance.Subscribe(HighHeavyEvent, Process);
        for (var i = 0; i < 5; i++)
        {
            EventBus.Instance.Subscribe(CrossEvent, Process);
            EventBus<GridMap2DData>.Instance.Subscribe(CrossEvent, Process);
        }
    }


    [Benchmark]
    public void LowEvent_Test() => EventBus.Instance.Publish(LowEvent);


    [Benchmark]
    public void HighEvent_Test() => EventBus.Instance.Publish(HighEvent);

    [Benchmark]
    public void HeavyEvent_Test() => EventBus<GridMap2DData>.Instance.Publish(HeavyEvent, _data);

    [Benchmark]
    public void HighHeavyEvent_Test() => EventBus<GridMap2DData>.Instance.Publish(HighHeavyEvent, _data);

    [Benchmark]
    public void CrossEvent_Test1() => EventBus.Instance.Publish(CrossEvent);

    [Benchmark]
    public void CrossEvent_Test2() => EventBus<GridMap2DData>.Instance.Publish(CrossEvent, _data);
}