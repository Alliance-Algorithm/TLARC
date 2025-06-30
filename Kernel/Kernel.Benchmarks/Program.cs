using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Kernel.Core.TransformTree;
using System.Numerics;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;

namespace Kernel.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        // var tmp = new TfCastBenchmarks();
        //
        // tmp.GlobalSetup();
        // while (true)
        //     tmp.CacheMiss_SamePath_NewValue();
        // var tmp = new EventBusBenchmarks();
        //
        // tmp.GlobalSetup();
        // while (true)
        //     tmp.HighHeavyEvent_Test();

        var sum = BenchmarkRunner.Run<TfCastBenchmarks>();
        return;
    }
}