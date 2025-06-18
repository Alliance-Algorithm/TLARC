using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Kernel.Core.TransformTree;
using System.Numerics;

namespace Kernel.Benchmarks;

[MemoryDiagnoser]
public class TfCastBenchmarks
{
    private const string Root = "root";
    private const string NodeA = "nodeA";
    private const string NodeB = "nodeB";
    private const string NodeC = "nodeC";
    private const string NodeD = "nodeD";

    [GlobalSetup]
    public void GlobalSetup()
    {
        Tf.AddTfNode(NodeA, Root);
        Tf.AddTfNode(NodeB, NodeA);
        Tf.AddTfNode(NodeC, NodeB);
        Tf.AddTfNode(NodeD, NodeC);

        // 设置初始变换
        Tf.SetTfNode(NodeA, new Vector3(1, 0, 0), Quaternion.Identity);
        Tf.SetTfNode(NodeB, new Vector3(0, 2, 0), Quaternion.Identity);
        Tf.SetTfNode(NodeC, new Vector3(0, 0, 3), Quaternion.Identity);
    }

    [Benchmark]
    public Vector3 CacheHit_ParentToChild()
    {
        // 使用缓存路径 (root->nodeA->nodeB)
        return Tf.Cast(NodeA, NodeB, Vector3.One);
    }

    [Benchmark]
    public Vector3 CacheMiss_NewPath()
    {
        // 强制创建新缓存路径
        Tf.SetTfNode(NodeD, new Vector3(0, 0, 4), Quaternion.Identity);
        return Tf.Cast(NodeD, Root, Vector3.One);
    }

    [Benchmark]
    public Vector3 DeepHierarchy()
    {
        // 测试深层级转换性能
        return Tf.Cast(NodeC, Root, Vector3.One);
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<TfCastBenchmarks>();
    }
}