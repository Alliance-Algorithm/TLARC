using System.Numerics;
using CostMap.Infrastructure.Algorithm;
using Kernel.DataInterfaces.Navigation;

namespace CostMap.Infrastructure.Data;

public class OccupancyHighGrid2DMap : IMap2D, IOccupancyGridMap2D
{
    public class DataInner : IOccupancyGridMap2DData
    {
        public IGridMap2DData GridData => DataChangable;
        public required Grid2DMap.DataInner DataChangable { get; init; }
        public sbyte Threshold { get; init; } = 70;
        public float LossFree { get; init; } = 0.7f;
        public float LossOccu { get; init; } = -1.9f;
        public required float[] OccupancyRate { get; set; }
    }

    public required float[] High { get; set; }

    public float ButtonZ { get; set; } = 0f;
    public float TopZ { get; set; } = 1f;
    public required DataInner DataChangeable { get; init; }

    public IOccupancyGridMap2DData OccupancyData => DataChangeable;
    public IMap2D Actions => this;

    public bool IsMoveAble(Vector2 from, Vector2 to) =>
        GridMapInner.CheckMoveable(from, DataChangeable.GridData, DataChangeable.Threshold,
            GridMapInner.ThresholdType.LessEqual);

    public bool IsMoveAble(Vector2 position) =>
        GridMapInner.CheckMoveable(position, DataChangeable.GridData, DataChangeable.Threshold,
            GridMapInner.ThresholdType.LessEqual);

    private OccupancyHighGrid2DMap() { }

    public static OccupancyHighGrid2DMap Build_IOccupancyGridMap2D(IOccupancyGridMap2D data) =>
        new()
        {
            DataChangeable = new DataInner
            {
                DataChangable = new Grid2DMap.DataInner
                {
                    HeaderData = new Grid2DMap.DataInner.HeaderInner(data.Header.Identifier),
                    Data = data.OccupancyData.GridData.Data, Height = data.OccupancyData.GridData.Height,
                    Width = data.OccupancyData.GridData.Width,
                    RotationRad = data.OccupancyData.GridData.RotationRad,
                    RotationMatrix = data.OccupancyData.GridData.RotationMatrix,
                    Origin = data.OccupancyData.GridData.Origin,
                    Resolution = data.OccupancyData.GridData.Resolution
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
            DataChangeable = new DataInner
            {
                DataChangable = new Grid2DMap.DataInner
                {
                    HeaderData = new Grid2DMap.DataInner.HeaderInner(data.Header.Identifier),
                    Data = data.GridData.Data, Height = data.GridData.Height,
                    Width = data.GridData.Width,
                    RotationRad = data.GridData.RotationRad,
                    RotationMatrix = data.GridData.RotationMatrix,
                    Origin = data.GridData.Origin,
                    Resolution = data.GridData.Resolution
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
            DataChangeable = new DataInner
            {
                DataChangable = new Grid2DMap.DataInner
                {
                    HeaderData = new Grid2DMap.DataInner.HeaderInner(data.Header.Identifier),
                    Data = data.Data, Height = data.Height,
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
}