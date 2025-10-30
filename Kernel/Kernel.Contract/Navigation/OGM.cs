using System.Numerics;

namespace Kernel.Contract.Navigation;

/// <summary>
/// Occupancy grid map
/// </summary>
public struct OGMData : ITlarcData
{
    /// <summary>
    ///  Inner Map Header and data
    /// </summary>
    public GridMap2DData GridMapData ;

    /// <summary>
    ///  if value &le; threshold then is free
    /// </summary>
    public sbyte Threshold;

    /// <summary>
    ///  空闲栅格增量
    /// </summary>
    public float LMiss;
    /// <summary>
    ///  占据栅格增量
    /// </summary>
    public float LHit;

    /// <summary>
    /// 
    /// </summary>
    public float[] LG ;
}