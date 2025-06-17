using g4;

namespace Kernel.DataInterfaces.Visualization;

public interface IRectangle : IShape
{
    /// <summary>
    /// 矩形在世界中的右下角位置
    /// </summary>
    public Vector2d Origin { get; }
    /// <summary>
    /// 地图大小：（width,height）
    /// <para>In 像素数量</para>
    /// index = y * width + x。
    /// </summary>
    public Vector2i Size { get; }
    /// <summary>
    /// 绕着右下角旋转的角度
    /// </summary>
    public double RotationRad { get; }
    /// <summary>
    /// 绕着右下角旋转的旋转矩阵
    /// </summary>
    public Matrix2d RotationMatrix { get; }

    /// <summary>
    /// 像素宽在真实世界中的大小
    /// <para>单位：m</para>
    /// </summary>
    public double Resolution { get; }
}
