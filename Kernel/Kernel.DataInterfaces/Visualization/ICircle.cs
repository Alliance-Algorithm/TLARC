using System.Numerics;

namespace Kernel.DataInterfaces.Visualization;

public interface ICircle : IShape
{
    /// <summary>
    /// 地图在世界中的右下角位置
    /// </summary>
    public Vector2 Origin { get; }
    /// <summary>
    /// 地图大小：（width,height）
    /// <para>In 像素数量</para>
    /// index = y * width + x。
    /// </summary>
    public Vector2 Size { get; }
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
    public double Resolution { get; }
}