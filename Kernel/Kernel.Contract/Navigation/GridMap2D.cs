using System.Numerics;

namespace Kernel.Contract.Navigation;


public struct GridMap2DData : ITlarcData
{
    public Header Header ;
    /// <summary>
    /// 地图在世界中的右下角位置
    /// </summary>
    public Vector2 Origin ;

    /// <summary>
    /// 地图宽 -> x
    /// <para>In 像素数量</para>
    /// index = y * width + x。
    /// </summary>
    public uint Width ;

    /// <summary>
    /// 地图高 -> y
    /// <para>In 像素数量</para>
    /// index = y * width + x。
    /// </summary>
    public uint Height ;

    /// <summary>
    /// 绕着右下角旋转的角度
    /// </summary>
    public double RotationRad ;

    /// <summary>
    /// 绕着右下角旋转的旋转矩阵
    /// </summary>
    public Matrix3x2 RotationMatrix ;

    /// <summary>
    /// 像素宽在真实世界中的大小
    /// <para>单位：m</para>
    /// </summary>
    public float Resolution ;

    /// <summary>
    /// 真实数据
    /// <para>单位：m</para>
    /// </summary>
    public sbyte[] Data ;
}
