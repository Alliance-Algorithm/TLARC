using System.Numerics;
using CostMap.Infrastructure.Algorithm;
using Kernel.DataInterfaces.Constraints;
using Kernel.DataInterfaces.Navigation;

namespace CostMap.Infrastructure.Data;



public class OccupancyGrid2DMap : IMap2D, IOccupancyGridMap2D
{

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

    private OccupancyGrid2DMap() { }

    public static OccupancyGrid2DMap Build_IOccupancyGridMap2D(IOccupancyGridMap2D data) =>
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
            }
        };

    public static OccupancyGrid2DMap Build_IOccupancyGridMap2DData(IOccupancyGridMap2DData data) =>
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
            }
        };

    public static OccupancyGrid2DMap Build_IGridMap2DData(IGridMap2DData data) =>
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
            }
        };
}