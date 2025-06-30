using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Kernel.Core.TransformTree;
using System.Numerics;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

namespace Kernel.Benchmarks;

// [SimpleJob(RuntimeMoniker.Net90)]
[RPlotExporter]
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
    public Vector3 SharedRandom() =>
        new(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle());

    [Benchmark]
    public Vector3 MatrixCalc() =>
        Vector3.Transform(
            new Vector3(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle()),
            Matrix4x4.CreateFromAxisAngle(Vector3.One, Random.Shared.NextSingle()));

    [Benchmark]
    public Vector3 CacheHit_Self() =>
        // 使用缓存路径 (root->nodeA->nodeB)
        Tf.Cast(NodeA, NodeA, Vector3.One);

    [Benchmark]
    public Vector3 CacheHit_ParentToChild() =>
        // 使用缓存路径 (root->nodeA->nodeB)
        Tf.Cast(NodeA, NodeB, Vector3.One);

    [Benchmark]
    public Vector3 CacheHit_ChildToParent() =>
        // 使用缓存路径 (root->nodeA->nodeB)
        Tf.Cast(NodeB, NodeA, Vector3.One);


    [Benchmark]
    public Vector3 CacheHit_SetPath()
    {
        // 强制创建新缓存路径
        Tf.SetTfNode(NodeD, new Vector3(0, 0, 4), Quaternion.Identity);
        return Tf.Cast(NodeD, Root, Vector3.One);
    }

    [Benchmark]
    public Vector3 DeepHierarchy() =>
        // 测试深层级转换性能
        Tf.Cast(NodeD, Root, Vector3.One);


    [Benchmark]
    public Vector3 CacheMiss_SamePath_NewValue()
    {
        // 强制创建新缓存路径
        Tf.SetTfNode(NodeA, new Vector3(0, Random.Shared.NextSingle(), 4), Quaternion.Identity);
        return Tf.Cast(NodeA, Root, Vector3.One);
    }

    private int i = 0;

    [Benchmark]
    public Vector3 CacheMiss_NewPath()
    {
        i = (i + 1) % 2;
        // 强制创建新缓存路径
        return i switch
        {
            0 => Tf.Cast(NodeD, Root, Vector3.One),
            1 => Tf.Cast(NodeC, Root, Vector3.One),
            _ => Tf.Cast(NodeB, Root, Vector3.One)
        };
    }

    [Benchmark]
    public Vector3 CacheMiss_NewPath_NewValue()
    {
        i = (i + 1) % 2;
        // 强制创建新缓存路径
        Tf.SetTfNode(NodeA, new Vector3(0, Random.Shared.NextSingle(), 4), Quaternion.Identity);
        return i switch
        {
            0 => Tf.Cast(NodeD, Root, Vector3.One),
            1 => Tf.Cast(NodeC, Root, Vector3.One),
            _ => Tf.Cast(NodeB, Root, Vector3.One)
        };
    }


    [GlobalCleanup]
    public void GlobalCleanup()
    {
    }
}