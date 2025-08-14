using System.Numerics;

namespace Kernel.DataInterfaces.Navigation;

public interface IOccupancyGridMap2DData : ITlarcData
{
    /// <summary>
    ///  Inner Map Header and data
    /// </summary>
    public IGridMap2DData GridMapData { get; }
    public IHeader Header => GridMapData.Header;
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

    public Vector2 Origin => GridMapData.Origin;

    /// <summary>
    /// 地图宽 -> x
    /// <para>In 像素数量</para>
    /// index = y * width + x。
    /// </summary>
    public uint Width => GridMapData.Width;

    /// <summary>
    /// 地图高 -> y
    /// <para>In 像素数量</para>
    /// index = y * width + x。
    /// </summary>
    public uint Height => GridMapData.Height;

    /// <summary>
    /// 绕着右下角旋转的角度
    /// </summary>
    public double RotationRad => GridMapData.RotationRad;

    /// <summary>
    /// 绕着右下角旋转的旋转矩阵
    /// </summary>
    public Matrix3x2 RotationMatrix => GridMapData.RotationMatrix;

    /// <summary>
    /// 像素宽在真实世界中的大小
    /// <para>单位：m</para>
    /// </summary>
    public float Resolution => GridMapData.Resolution;

    /// <summary>
    /// 真实数据
    /// <para>单位：m</para>
    /// </summary>
    public sbyte[] Data => GridMapData.Data;
}

public interface IOccupancyGridMap2D
{
    public IHeader Header => OccupancyData.GridMapData.Header;
    public IMap2D Actions { get; }
    public IOccupancyGridMap2DData OccupancyData { get; }
}