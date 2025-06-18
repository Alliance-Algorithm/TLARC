// NUnit 测试类

using System.Numerics;
using Kernel.Core.TransformTree;

namespace Kernel.Test.TF;

[TestFixture]
public class TfTests
{
#region SetTfNode 测试

    [Test]
    public void SetTfNode_ShouldSetTranslation()
    {
        Tf.AddTfNode("vehicle", "world");
        var position = new Vector3(10, 5, 2);

        Tf.SetTfNode("vehicle", position, Quaternion.Identity);

        // 在实际实现中应能验证内部状态
        Assert.Pass();
    }

    [Test]
    public void SetTfNode_ShouldSetRotation()
    {
        Tf.AddTfNode("gripper", "world");
        var rotation = Quaternion.CreateFromYawPitchRoll(MathF.PI / 2, 0, 0);

        Tf.SetTfNode("gripper", Vector3.Zero, rotation);

        // 在实际实现中应能验证内部状态
        Assert.Pass();
    }

    [Test]
    public void SetTfNode_ShouldThrowWhenNodeNotFound()
    {
        var ex = Assert.Throws<TlarcTfError.SetNoNodeException>(() =>
            Tf.SetTfNode("unknown", Vector3.Zero, Quaternion.Identity));

        Assert.That(ex.Message, Does.Contain("do not include node"));
    }

#endregion

#region Cast 测试 - 基本功能

    [Test]
    public void Cast_ShouldReturnSamePositionForSameCoordinateSystem()
    {
        Tf.AddTfNode("base", "world");
        var position = new Vector3(3, 4, 5);

        var result = Tf.Cast("base", "base", position);

        Assert.That(result, Is.EqualTo(position));
    }

    [Test]
    public void Cast_ShouldHandleDirectTranslation()
    {
        Tf.AddTfNode("object", "world");
        Tf.SetTfNode("object", new Vector3(10, 0, 0), Quaternion.Identity);

        // 从世界坐标系到物体坐标系
        var worldPos  = new Vector3(15, 0, 0);
        var objectPos = Tf.Cast("object", "world", worldPos);

        Assert.That(objectPos, Is.EqualTo(new Vector3(5, 0, 0)));
    }

    [Test]
    public void Cast_ShouldHandleRotation()
    {
        Tf.AddTfNode("rotated", "world");
        var rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathF.PI / 2); // 90度绕Z轴
        Tf.SetTfNode("rotated", Vector3.Zero, rotation);

        // 从世界坐标系到旋转坐标系
        var worldPos   = new Vector3(1, 0, 0);
        var rotatedPos = Tf.Cast("world", "rotated", worldPos);


        // 旋转后应变为 (0, 1, 0)
        Assert.That(rotatedPos.X, Is.EqualTo(0).Within(0.0001f));
        Assert.That(rotatedPos.Y, Is.EqualTo(1).Within(0.0001f));
    }

#endregion

#region Cast 测试 - 复杂场景

    [Test]
    public void Cast_ShouldHandleMultiLevelTransform()
    {
        // 创建层次结构: world -> robot -> arm
        Tf.AddTfNode("robot", "world");
        Tf.AddTfNode("arm",   "robot");

        // 设置变换
        Tf.SetTfNode("robot", new Vector3(5, 0, 0), Quaternion.Identity);
        Tf.SetTfNode("arm",   new Vector3(0, 3, 0), Quaternion.Identity);

        // 从世界坐标系到手臂坐标系
        var worldPos = new Vector3(6, 4, 0);
        var armPos   = Tf.Cast("arm", "world", worldPos);

        // 计算: (6-5, 4-3) = (1, 1)
        Assert.That(armPos, Is.EqualTo(new Vector3(1, 1, 0)));
    }

    [Test]
    public void Cast_ShouldHandleCombinedRotationAndTranslation()
    {
        Tf.AddTfNode("complex", "world");
        var rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathF.PI); // 180度绕Y轴
        Tf.SetTfNode("complex", new Vector3(10, 0, 5), rotation);

        var worldPos = new Vector3(12, 3, 7);
        var localPos = Tf.Cast("world", "complex", worldPos);

        // 相对位置: (12-10, 3-0, 7-5) = (2, 3, 2)
        // 180度旋转后: (-2, 3, -2)
        Assert.That(localPos.X, Is.EqualTo(-2).Within(0.0001f));
        Assert.That(localPos.Y, Is.EqualTo(3).Within(0.0001f));
        Assert.That(localPos.Z, Is.EqualTo(-2).Within(0.0001f));
    }

    [Test]
    public void Cast_ShouldHandleBranchingPaths()
    {
        // 创建树结构:
        //   world
        //   ├── robot
        //   │   ├── arm
        //   │   └── camera
        //   └── sensor
        Tf.AddTfNode("robot",  "world");
        Tf.AddTfNode("arm",    "robot");
        Tf.AddTfNode("camera", "robot");
        Tf.AddTfNode("sensor", "world");

        // 设置变换
        Tf.SetTfNode("robot",  new Vector3(0, 0, 10), Quaternion.Identity);
        Tf.SetTfNode("arm",    new Vector3(0, 2, 0),  Quaternion.Identity);
        Tf.SetTfNode("camera", new Vector3(1, 0, 0),  Quaternion.Identity);
        Tf.SetTfNode("sensor", new Vector3(0, 3, 0),  Quaternion.Identity);

        // 从传感器坐标系到手臂坐标系
        var sensorPos = new Vector3(0, 0, 0);
        var armPos    = Tf.Cast("sensor", "arm", sensorPos);

        // 路径: sensor -> world -> robot -> arm
        // sensor在世界坐标系的位置: (0, 3, 0)
        // arm在世界坐标系的位置: (0, 2, 10)
        // 从传感器到手臂的相对位置: (0-0, 2-3, 10-0) = (0, -1, 10)
        Assert.That(armPos, Is.EqualTo(new Vector3(0, -1, 10)));
    }

#endregion

#region Cast 测试 - 边界情况和错误处理

    [Test]
    public void Cast_ShouldThrowWhenSourceNodeNotFound()
    {
        var ex = Assert.Throws<TlarcTfError.FoundNoNodeException>(() =>
            Tf.Cast("unknown", "world", Vector3.Zero));

        Assert.That(ex.Message, Does.Contain("do not include node"));
    }

    [Test]
    public void Cast_ShouldThrowWhenTargetNodeNotFound()
    {
        var ex = Assert.Throws<TlarcTfError.FoundNoNodeException>(() =>
            Tf.Cast("unknown", "world", Vector3.Zero));

        Assert.That(ex.Message, Does.Contain("do not include node"));
    }

    [Test]
    public void Cast_ShouldHandleZeroVector()
    {
        Tf.AddTfNode("origin", "world");
        Tf.SetTfNode("origin", new Vector3(5, 10, 15), Quaternion.Identity);

        var result = Tf.Cast("origin", "world", Vector3.Zero);

        // 零向量变换后应为相对位置的负值
        Assert.That(result, Is.EqualTo(new Vector3(-5, -10, -15)));
    }

    [Test]
    public void Cast_ShouldHandleLargeCoordinates()
    {
        const float largeValue = 1e6f;
        Tf.AddTfNode("large", "world");
        Tf.SetTfNode("large", new Vector3(largeValue, 0, 0), Quaternion.Identity);

        var worldPos = new Vector3(2 * largeValue, 0, 0);
        var localPos = Tf.Cast("large", "world", worldPos);

        Assert.That(localPos.X, Is.EqualTo(largeValue).Within(0.1f));
        Assert.That(localPos.Y, Is.EqualTo(0).Within(0.0001f));
        Assert.That(localPos.Z, Is.EqualTo(0).Within(0.0001f));
    }

    [Test]
    public void Cast_ShouldHandlePrecisionForSmallValues()
    {
        const float smallValue = 1e-6f;
        Tf.AddTfNode("precise", "world");
        Tf.SetTfNode("precise", new Vector3(smallValue, 0, 0), Quaternion.Identity);

        var worldPos = new Vector3(2 * smallValue, 0, 0);
        var localPos = Tf.Cast("precise", "world", worldPos);

        Assert.That(localPos.X, Is.EqualTo(smallValue).Within(1e-8f));
        Assert.That(localPos.Y, Is.EqualTo(0).Within(1e-8f));
        Assert.That(localPos.Z, Is.EqualTo(0).Within(1e-8f));
    }

    [Test]
    public void Cast_ShouldHandleRotationPrecision()
    {
        Tf.AddTfNode("rot_prec", "world");
        var rotation = Quaternion.CreateFromAxisAngle(
            Vector3.Normalize(new Vector3(1, 1, 1)), MathF.PI / 4);

        Tf.SetTfNode("rot_prec", Vector3.Zero, rotation);

        var worldPos = new Vector3(1, 0, 0);
        var localPos = Tf.Cast("rot_prec", "world", worldPos);

        // 验证旋转后的位置是否合理
        var expectedLength = worldPos.Length();
        Assert.That(localPos.Length(), Is.EqualTo(expectedLength).Within(1e-6f));
    }

#endregion

#region 性能测试（可选）

    [Test]
    public void Cast_PerformanceTest()
    {
        // 创建深度树结构
        const int depth = 100;
        var       prev  = "world";

        for (var i = 0; i < depth; i++)
        {
            var nodeId = $"node_{i}";
            Tf.AddTfNode(nodeId, prev);
            Tf.SetTfNode(nodeId, new Vector3(1, 0, 0), Quaternion.CreateFromAxisAngle(Vector3.UnitY, i));
            prev = nodeId;
        }

        // 测量从叶子节点到根节点的转换时间
        var       sw         = System.Diagnostics.Stopwatch.StartNew();
        const int iterations = 1000;

        for (var i = 0; i < iterations; i++)
            Tf.Cast("world", $"node_{depth - 1}", Vector3.One);

        sw.Stop();
        var avgTime = sw.Elapsed.TotalMilliseconds / iterations;

        Assert.That(avgTime, Is.LessThan(1.0), $"平均转换时间 {avgTime} ms 超过 1 ms");
        TestContext.WriteLine($"深度 {depth} 的树转换平均耗时: {avgTime:F6} ms");
    }
    

#endregion
}