using System.Numerics;
using CostMap.Infrastructure.Algorithm;
using Kernel.Contract.Constraints;
using Kernel.Contract.Navigation;

namespace CostMap.Infrastructure.Data;



public class OccupancyGrid2DMap : IMap2D
{

    public float ButtonZ { get; set; } = 0f;
    public float TopZ { get; set; } = 1f;
    public required OGMData Data;

    public bool IsMoveAble(Vector2 from, Vector2 to) =>
        GridMapInner.CheckMoveable(from, Data.GridMapData, Data.Threshold,
            GridMapInner.ThresholdType.LessEqual);

    public bool IsMoveAble(Vector2 position) =>
        GridMapInner.CheckMoveable(position, Data.GridMapData, Data.Threshold,
            GridMapInner.ThresholdType.LessEqual);

    private OccupancyGrid2DMap() { }


    public static OccupancyGrid2DMap Build_IOccupancyGridMap2DData(OGMData data) =>
        new()
        {
            Data = data
        };

    public static OccupancyGrid2DMap Build_GridMap2DData(GridMap2DData data) =>
        new()
        {
            Data = new OGMData
            {
                GridMapData = data,
                LG = new float[data.Header.Width * data.Header.Height]
            }
        };
    public static OccupancyGrid2DMap Build_GridMap2DData(GridMap2DDescription data) =>
        new()
        {
            Data = new OGMData
            {
                GridMapData = new(){Header = data, Data = new sbyte[data.Width * data.Height]},
                LG = new float[data.Width * data.Height]
            }
        };
}