```

BenchmarkDotNet v0.15.2, Linux Ubuntu 22.04.3 LTS (Jammy Jellyfish)
12th Gen Intel Core i7-12700H, 1 CPU, 20 logical and 10 physical cores
.NET SDK 9.0.107
  [Host] : .NET 9.0.6 (9.0.625.26613), X64 AOT AVX2

Job=NativeAOT 9.0  Runtime=NativeAOT 9.0  

```
| Method       | Mean | Error | Ratio | RatioSD |
|------------- |-----:|------:|------:|--------:|
| SharedRandom |   NA |    NA |     ? |       ? |

Benchmarks with issues:
  TfCastBenchmarks.SharedRandom: NativeAOT 9.0(Runtime=NativeAOT 9.0)
