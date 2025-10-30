using System.Numerics;
using CostMap.Infrastructure.Algorithm;
using Kernel.Contract.Navigation;

namespace CostMap.Infrastructure.Data;

public class OccupancyHighGrid2DMap : IMap2D
{
    public required float[] High { get; set; }

    public float ButtonZ { get; set; } = 0f;
    public float TopZ { get; set; } = 1f;
    public required OGMData Data;

    public bool IsMoveAble(Vector2 from, Vector2 to) =>
        GridMapInner.CheckMoveable(from, Data.GridMapData, Data.Threshold,
            GridMapInner.ThresholdType.LessEqual);

    public bool IsMoveAble(Vector2 position) =>
        GridMapInner.CheckMoveable(position, Data.GridMapData, Data.Threshold,
            GridMapInner.ThresholdType.LessEqual);

    internal OccupancyHighGrid2DMap() { }

    public static OccupancyHighGrid2DMap Build_IOccupancyGridMap2DData(OGMData data) =>
        new()
        {
            Data = data,
            High = new float[data.GridMapData.Width * data.GridMapData.Height]
        };

    public static OccupancyHighGrid2DMap Build_GridMap2DData(GridMap2DData data) =>
        new()
        {
            Data = new OGMData
            {
                GridMapData = data,
                LG = new float[data.Width * data.Height]
            },
            High = new float[data.Width * data.Height]
        };

    public static OccupancyHighGrid2DMap Build_Clone(OccupancyHighGrid2DMap map) =>
        new()
        {
            Data = new ()
            {
                GridMapData = new()
                {
                    Header = map.Data.GridMapData.Header,
                    Data = map.Data.GridMapData.Data[..],
                    Height = map.Data.GridMapData.Height,
                    Width = map.Data.GridMapData.Width,
                    RotationRad = map.Data.GridMapData.RotationRad,
                    RotationMatrix = map.Data.GridMapData.RotationMatrix,
                    Origin = map.Data.GridMapData.Origin,
                    Resolution = map.Data.GridMapData.Resolution
                },
                LG = map.Data.LG[..]
            },
            High = map.High[..]
        };
}