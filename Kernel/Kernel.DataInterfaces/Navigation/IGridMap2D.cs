using System.Numerics;

namespace Kernel.DataInterfaces.Navigation;

public interface IGridMap2DData : ITlarcData, ITransform
{
    /// <summary>
    /// 地图在世界中的右下角位置
    /// </summary>
    public Vector2 Origin { get; }

    /// <summary>
    /// 地图宽 -> x
    /// <para>In 像素数量</para>
    /// index = y * width + x。
    /// </summary>
    public uint Width { get; }

    /// <summary>
    /// 地图高 -> y
    /// <para>In 像素数量</para>
    /// index = y * width + x。
    /// </summary>
    public uint Height { get; }

    /// <summary>
    /// 绕着右下角旋转的角度
    /// </summary>
    public double RotationRad { get; }

    /// <summary>
    /// 绕着右下角旋转的旋转矩阵
    /// </summary>
    public Matrix3x2 RotationMatrix { get; }

    /// <summary>
    /// 像素宽在真实世界中的大小
    /// <para>单位：m</para>
    /// </summary>
    public float Resolution { get; }

    /// <summary>
    /// 真实数据
    /// <para>单位：m</para>
    /// </summary>
    public sbyte[] Data { get; }
}

public interface IGridMap2D : IGridMap2DData, IMap2D;