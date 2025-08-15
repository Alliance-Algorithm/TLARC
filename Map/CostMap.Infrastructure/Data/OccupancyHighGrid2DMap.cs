using System.Numerics;
using CostMap.Infrastructure.Algorithm;
using Kernel.DataInterfaces.Navigation;

namespace CostMap.Infrastructure.Data;

public class OccupancyHighGrid2DMap : IMap2D, IOccupancyGridMap2D
{
    public required float[] High { get; set; }

    public float ButtonZ { get; set; } = 0f;
    public float TopZ { get; set; } = 1f;
    public required OccupancyGridData DataChangeable { get; init; }

    public IOccupancyGridMap2DData OccupancyData => DataChangeable;
    public IMap2D Actions => this;

    public bool IsMoveAble(Vector2 from, Vector2 to) =>
        GridMapInner.CheckMoveable(from, DataChangeable.GridMapData, DataChangeable.Threshold,
            GridMapInner.ThresholdType.LessEqual);

    public bool IsMoveAble(Vector2 position) =>
        GridMapInner.CheckMoveable(position, DataChangeable.GridMapData, DataChangeable.Threshold,
            GridMapInner.ThresholdType.LessEqual);

    internal OccupancyHighGrid2DMap() { }

    public static OccupancyHighGrid2DMap Build_IOccupancyGridMap2D(IOccupancyGridMap2D data) =>
        new()
        {
            DataChangeable = new OccupancyGridData
            {
                DataChangeable = new Grid2DMapData
                {
                    HeaderData = new Grid2DMapData.HeaderInner(data.Header.Identifier),
                    Data = data.OccupancyData.GridMapData.Data,
                    Height = data.OccupancyData.GridMapData.Height,
                    Width = data.OccupancyData.GridMapData.Width,
                    RotationRad = data.OccupancyData.GridMapData.RotationRad,
                    RotationMatrix = data.OccupancyData.GridMapData.RotationMatrix,
                    Origin = data.OccupancyData.GridMapData.Origin,
                    Resolution = data.OccupancyData.GridMapData.Resolution
                },
                LossFree = data.OccupancyData.LossFree,
                LossOccu = data.OccupancyData.LossOccu,
                Threshold = data.OccupancyData.Threshold,
                OccupancyRate = data.OccupancyData.OccupancyRate
            },
            High = new float[data.OccupancyData.Width * data.OccupancyData.Height]
        };

    public static OccupancyHighGrid2DMap Build_IOccupancyGridMap2DData(IOccupancyGridMap2DData data) =>
        new()
        {
            DataChangeable = new OccupancyGridData
            {
                DataChangeable = new Grid2DMapData
                {
                    HeaderData = new Grid2DMapData.HeaderInner(data.Header.Identifier),
                    Data = data.GridMapData.Data,
                    Height = data.GridMapData.Height,
                    Width = data.GridMapData.Width,
                    RotationRad = data.GridMapData.RotationRad,
                    RotationMatrix = data.GridMapData.RotationMatrix,
                    Origin = data.GridMapData.Origin,
                    Resolution = data.GridMapData.Resolution
                },
                LossFree = data.LossFree,
                LossOccu = data.LossOccu,
                Threshold = data.Threshold,
                OccupancyRate = data.OccupancyRate
            },
            High = new float[data.Width * data.Height]
        };

    public static OccupancyHighGrid2DMap Build_IGridMap2DData(IGridMap2DData data) =>
        new()
        {
            DataChangeable = new OccupancyGridData
            {
                DataChangeable = new Grid2DMapData
                {
                    HeaderData = new Grid2DMapData.HeaderInner(data.Header.Identifier),
                    Data = data.Data,
                    Height = data.Height,
                    Width = data.Width,
                    RotationRad = data.RotationRad,
                    RotationMatrix = data.RotationMatrix,
                    Origin = data.Origin,
                    Resolution = data.Resolution
                },
                OccupancyRate = new float[data.Width * data.Height]
            },
            High = new float[data.Width * data.Height]
        };

    public static OccupancyHighGrid2DMap Build_Clone(OccupancyHighGrid2DMap data) =>
        new()
        {
            DataChangeable = new OccupancyGridData
            {
                DataChangeable = new Grid2DMapData
                {
                    HeaderData =
                        new Grid2DMapData.HeaderInner(data.DataChangeable.DataChangeable.Header.Identifier),
                    Data = data.DataChangeable.DataChangeable.Data[..],
                    Height = data.DataChangeable.DataChangeable.Height,
                    Width = data.DataChangeable.DataChangeable.Width,
                    RotationRad = data.DataChangeable.DataChangeable.RotationRad,
                    RotationMatrix = data.DataChangeable.DataChangeable.RotationMatrix,
                    Origin = data.DataChangeable.DataChangeable.Origin,
                    Resolution = data.DataChangeable.DataChangeable.Resolution
                },
                OccupancyRate = data.OccupancyData.OccupancyRate[..]
            },
            High = data.High[..]
        };
}