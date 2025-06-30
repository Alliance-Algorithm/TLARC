using NUnit.Framework;
using System;
using System.Numerics;
using Kernel.Core.TransformTree;

namespace Kernel.Test.TF;

[TestFixture]
public class TfTest
{
    // 定义7个坐标系节点
    private const string World = "world";
    private const string Base = "base";
    private const string Arm = "arm";
    private const string Tool = "tool";
    private const string Camera = "camera";
    private const string Object = "object";
    private const string Sensor = "sensor";

    [SetUp]
    public void Setup()
    {
        // 构建5级变换树（符合ROS TF规则）
        // world (根节点)
        Tf.AddTfNode(World, World);

        // base_frame 是 world 的子节点
        Tf.AddTfNode(Base, World);
        Tf.SetTfNode(Base, new Vector3(1, 0, 0),
            Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathF.PI / 2)); // 绕Z轴旋转90度

        // arm 和 camera 是 base_frame 的子节点
        Tf.AddTfNode(Arm, Base);
        Tf.SetTfNode(Arm, new Vector3(0, 2, 0),
            Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathF.PI / 4)); // 绕X轴旋转45度

        Tf.AddTfNode(Camera, Base);
        Tf.SetTfNode(Camera, new Vector3(0, 0, 1),
            Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathF.PI / 3)); // 绕Y轴旋转60度

        // tool 是 arm 的子节点
        Tf.AddTfNode(Tool, Arm);
        Tf.SetTfNode(Tool, new Vector3(0, 0, 3),
            Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathF.PI / 6)); // 绕Z轴旋转30度

        // object 是 camera 的子节点
        Tf.AddTfNode(Object, Camera);
        Tf.SetTfNode(Object, new Vector3(0.5f, 0.5f, 0.5f),
            Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathF.PI / 3)); // 绕X轴旋转60度

        // sensor 是 tool 的子节点
        Tf.AddTfNode(Sensor, Tool);
        Tf.SetTfNode(Sensor, new Vector3(0, 1, 0),
            Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathF.PI / 4)); // 绕Y轴旋转45度
    }

    // 容差精度
    private const float Tolerance = 1e-5f;

    // 测试点
    private static readonly Vector3 TestPoint = new(1, 2, 3);

    // 辅助函数：验证正反变换一致性
    private void TestConsistentTransform(string from, string to)
    {
        // 正向变换: from -> to
        var transformed = Tf.Cast(from, to, TestPoint);

        // 反向变换: to -> from
        var backTransformed = Tf.Cast(to, from, transformed);

        // 验证来回变换后点坐标应恢复原值
        Assert.That(backTransformed.X, Is.EqualTo(TestPoint.X).Within(Tolerance),
            $"{from}->{to}->{from} transform consistency failed (X)");
        Assert.That(backTransformed.Y, Is.EqualTo(TestPoint.Y).Within(Tolerance),
            $"{from}->{to}->{from} transform consistency failed (Y)");
        Assert.That(backTransformed.Z, Is.EqualTo(TestPoint.Z).Within(Tolerance),
            $"{from}->{to}->{from} transform consistency failed (Z)");
    }

    // 验证已知变换路径
    private void TestKnownTransform(string from, string to, Vector3 expected)
    {
        var result = Tf.Cast(from, to, TestPoint);

        Assert.That(result.X, Is.EqualTo(expected.X).Within(Tolerance),
            $"{from}->{to} transform X mismatch: {result.X} vs {expected.X}");
        Assert.That(result.Y, Is.EqualTo(expected.Y).Within(Tolerance),
            $"{from}->{to} transform Y mismatch: {result.Y} vs {expected.Y}");
        Assert.That(result.Z, Is.EqualTo(expected.Z).Within(Tolerance),
            $"{from}->{to} transform Z mismatch: {result.Z} vs {expected.Z}");
    }

    // 1. 测试直接变换路径
    [Test]
    public void Cast_DirectPaths()
    {
        // World -> Base (符合ROS TF规则)
        // Base相对于World: 位置(1,0,0), 旋转90°Z
        // 点(1,2,3)在World系中:
        //  2. 减去平移: (1-1, 2, 3) = (0,2,3)
        //  3. 旋转后: x' = 0*cos(-90) - 2*sin(-90) = 0*0 - 2*(-1) = 2
        //             y' = 0*sin(-90) + 2*cos(-90) = 0*(-1) + 2*0 = 0
        //             z' = 3
        TestKnownTransform(World, Base, new Vector3(2, 0, 3));

        // Base -> Arm
        // Arm相对于Base: 位置(0,2,0), 旋转45°X
        // 点(1,2,3)在Base系中:
        //  1. 减去平移: (1, 2-2, 3) = (1,0,3)
        //  2. 应用逆旋转: -45°X
        //     y' = 0*cos(-45) - 3*sin(-45) = 0*0.707 - 3*(-0.707) ≈ 2.121
        //     z' = 0*sin(-45) + 3*cos(-45) = 0*(-0.707) + 3*0.707 ≈ 2.121
        TestKnownTransform(Base, Arm, new Vector3(1, 2.12132f, 2.12132f));

        // Arm -> Tool
        // Tool相对于Arm: 位置(0,0,3), 旋转30°Z
        // 点(1,2,3)在Arm系中:
        //  1. 减去平移: (1,2,3-3) = (1,2,0)
        //  2. 应用逆旋转: -30°Z
        //     x' = 1*cos(-30) - 2*sin(-30) ≈ 1*0.866 - 2*(-0.5) = 0.866 + 1 = 1.866
        //     y' = 1*sin(-30) + 2*cos(-30) ≈ 1*(-0.5) + 2*0.866 = -0.5 + 1.732 = 1.232
        TestKnownTransform(World, Sensor, new Vector3(-0.423161983f, 1.7247448f, 0.1805207811f));
    }

    // 2. 测试多级变换路径
    [Test]
    public void Cast_MultiLevelPaths()
    {
        // Base -> World (Base->World)
        // 预计算值
        TestKnownTransform(Base, World, new Vector3(-1, 1, 3));

        // Tool -> Base (Tool->Arm->Base)
        TestKnownTransform(Tool, Base, new Vector3(-0.133974612f, -0.66434288f, 5.82093859f));

        // Sensor -> World (Sensor->Tool->Arm->Base->World)
        TestKnownTransform(Sensor, World, new Vector3(-0.715796351f, 0.949489653f, 5.95843744f));
    }

    // 3. 测试分支间变换
    [Test]
    public void Cast_CrossBranchPaths()
    {
        // Tool -> Camera (Tool->Arm->Base->Camera)
        TestKnownTransform(Tool, Camera, new Vector3(0.5f, 3.73205f, 1.23205f));

        // Object -> Arm (Object->Camera->Base->Arm)
        TestKnownTransform(Object, Arm, new Vector3(-0.5f, 0.68301f, 0.68301f));

        // Sensor -> Object (Sensor->Tool->Arm->Base->Camera->Object)
        TestKnownTransform(Sensor, Object, new Vector3(0.70711f, 0.80301f, -0.80301f));
    }

    // 4. 测试所有节点对组合（7x7=49种组合）
    [Test]
    public void Cast_AllPairsConsistency()
    {
        string[] nodes = { World, Base, Arm, Tool, Sensor, Camera, Object };

        foreach (var from in nodes)
        foreach (var to in nodes)
            TestConsistentTransform(from, to);
    }

    // 5. 测试反向变换
    [Test]
    public void Cast_ReverseTransforms()
    {
        // 验证具体反向变换值
        // Base -> World 应该是 World -> Base 的逆
        TestKnownTransform(Base, World, new Vector3(-1, 1, 3));

        // 验证反向变换与正向变换互逆
        TestConsistentTransform(World, Base);
        TestConsistentTransform(Base,  Object);
        TestConsistentTransform(Arm,   Camera);
        TestConsistentTransform(Tool,  Sensor);
    }

    // 6. 测试相同坐标系
    [Test]
    public void Cast_SameCoordinateSystem()
    {
        foreach (var frame in new[] { World, Base, Arm, Tool, Sensor, Camera, Object })
        {
            var result = Tf.Cast(frame, frame, TestPoint);
            Assert.That(result, Is.EqualTo(TestPoint));
        }
    }
}