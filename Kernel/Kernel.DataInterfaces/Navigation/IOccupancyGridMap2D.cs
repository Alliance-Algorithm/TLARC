using System.Numerics;

namespace Kernel.DataInterfaces.Navigation;

public interface IOccupancyGridMap2DData : ITlarcData
{
    public IGridMap2DData GridData { get; }
    public IHeader Header => GridData.Header;
    /// <summary>
    ///  if value &le; threshold then is free
    /// </summary>
    public sbyte Threshold { get; }

    /// <summary>
    ///  空闲栅格增量
    /// </summary>
    public float LossFree { get; }
    /// <summary>
    ///  占据栅格增量
    /// </summary>
    public float LossOccu { get; }

    public float[] OccupancyRate { get; }

    public Vector2 Origin => GridData.Origin;

    /// <summary>
    /// 地图宽 -> x
    /// <para>In 像素数量</para>
    /// index = y * width + x。
    /// </summary>
    public uint Width => GridData.Width;

    /// <summary>
    /// 地图高 -> y
    /// <para>In 像素数量</para>
    /// index = y * width + x。
    /// </summary>
    public uint Height => GridData.Height;

    /// <summary>
    /// 绕着右下角旋转的角度
    /// </summary>
    public double RotationRad => GridData.RotationRad;

    /// <summary>
    /// 绕着右下角旋转的旋转矩阵
    /// </summary>
    public Matrix3x2 RotationMatrix => GridData.RotationMatrix;

    /// <summary>
    /// 像素宽在真实世界中的大小
    /// <para>单位：m</para>
    /// </summary>
    public float Resolution => GridData.Resolution;

    /// <summary>
    /// 真实数据
    /// <para>单位：m</para>
    /// </summary>
    public sbyte[] Data => GridData.Data;
}

public interface IOccupancyGridMap2D
{
    public IHeader Header => OccupancyData.GridData.Header;
    public IMap2D Actions { get; }
    public IOccupancyGridMap2DData OccupancyData { get; }
}