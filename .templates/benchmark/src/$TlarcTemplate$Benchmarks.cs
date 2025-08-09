using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

namespace Kernel.Benchmarks;

[RPlotExporter]
public class $TlarcTemplate$Benchmarks
{

    [GlobalSetup]
    public void GlobalSetup()
    {
    }

    // [Benchmark]
    // public Vector3 Function() {}


    [GlobalCleanup]
    public void GlobalCleanup()
    {
    }
}