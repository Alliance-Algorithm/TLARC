using System.Numerics;

namespace Kernel.DataInterfaces.Navigation;

public interface IGridMap2DData : ITlarcData
{
    IHeader Header { get; }
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

public interface IGridMap2D : ITlarcData
{
    public IGridMap2DData Data { get; }
    public IMap2D Actions { get; }

    /// <summary>
    /// 从from 到 to 是否可以移动
    /// </summary>
    /// <param name="from">起始位置</param>
    /// <param name="to">终点位置</param>
    /// <returns>如果有可以直线通过：true</returns>
    public bool IsMoveAble(in int fromX, in int fromY, in int toX, in int toY);

    /// <summary>
    /// position是否有障碍物
    /// </summary>
    /// <param name="position">tlarc坐标系坐标</param>
    /// <returns>如果有障碍物：true</returns>
    public bool IsMoveAble(in int positionX, in int positionY);
}