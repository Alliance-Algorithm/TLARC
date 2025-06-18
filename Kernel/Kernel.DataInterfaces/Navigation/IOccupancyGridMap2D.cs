namespace Kernel.DataInterfaces.Navigation;

public interface IOccupancyGridMap2DData : IGridMap2DData
{
    /// <summary>
    ///  if value &le; threshold then is free
    /// </summary>
    public sbyte Threshold { get; }
}

public interface IOccupancyGridMap2D : IOccupancyGridMap2DData, IMap2D;