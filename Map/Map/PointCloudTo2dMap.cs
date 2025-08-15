using System.Numerics;
using System.Runtime.CompilerServices;
using CostMap.Infrastructure.Data;
using Kernel.Core.EventBus;
using Kernel.Core.TransformTree;
using Kernel.DataInterfaces;
using Kernel.DataInterfaces.Navigation;
using Kernel.DataInterfaces.Sensor;

namespace Map;

public class PointCloudTo2dMap
{
# region 类型定义

    private class InnerData : IOccupancyGridMap2DData, IHeader, IGridMap2DData
    {
        public IHeader Header => this;
        public IGridMap2DData GridData => this;
        public string Identifier { get; set; } = "";
        public Vector2 Origin { get; } = new();
        public uint Width { get; set; } = 100;
        public uint Height { get; set; } = 100;
        public double RotationRad { get; set; } = 0;
        public Matrix3x2 RotationMatrix { get; set; } = Matrix3x2.Identity;
        public float Resolution { get; set; } = 0.02f;
        public sbyte[] Data { get; set; } = [];
        public float[] OccupancyRate { get; set; } = [];
        public sbyte Threshold { get; set; } = 70;
        public float LossFree { get; set; } = 0.7f;
        public float LossOccu { get; set; } = -0.9f;
        public float BottomZ { get; set; } = 0.01f;
        public float TopZ { get; set; } = 0.08f;
    }

#endregion

#region 局部变量

    private string _pointCloudTopicName;
    private string _costMapTopicName;
    private string _staticMapTopicName;

    private string _pointCloudId = "lidar_link";
    private string _sensorId = "lidar_link";
    private string _chassisId = "car_link";
    private string _costMapId = "cost_map_link";

    private OccupancyGrid2DMap? _innerMap;
    private OccupancyHighGrid2DMap? _inner25DMap;

    private readonly InnerData _innerData;

#endregion

    private PointCloudTo2dMap(string pointCloudTopicName, string costMapTopicName, string staticMapTopicName)
    {
        _pointCloudTopicName = pointCloudTopicName;
        _costMapTopicName = costMapTopicName;
        _staticMapTopicName = staticMapTopicName;
        _innerData = new InnerData();
    }

# region 公共设置接口

    /// <summary>
    /// <para></para> pointCloudTopicName = "/tlarc/point_cloud/segment"
    /// <para></para> costMapTopicName = "/tlarc/map/cost_map_with_pcd"
    /// <para></para> staticMapTopicName = "/tlarc/map_server/static_map"
    /// <para></para> uint  width      = 100,
    /// <para></para> uint  height     = 100,
    /// <para></para> sbyte threshold  = 70,
    /// <para></para> float resolution = 60f,
    /// <para></para> float lossFree   = 0.7f,
    /// <para></para> float lossOccu   = -0.9f,
    /// <para></para> float buttonZ    = -0.5f,
    /// <para></para> float topZ       = 0.1f
    /// </summary>
    public static PointCloudTo2dMap DefaultNew
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new("/tlarc/point_cloud/segment", "/tlarc/map/cost_map_with_pcd", "/tlarc/map_server/static_map");
    }

    /// <summary>
    /// 在事件系统中注册所有的事件
    /// </summary>
    /// <remarks>
    /// <para>输入：</para>
    /// <para>PointCloudInterface ->  Kernel.DataInterfaces.Sensor.IPointCloud</para>
    /// </remarks>
    /// <returns></returns>
    public PointCloudTo2dMap BuildOccupancyMapWithStaticHigh()
    {
        _innerData.Data = new sbyte[_innerData.Width           * _innerData.Height];
        _innerData.OccupancyRate = new float [_innerData.Width * _innerData.Height];
        Array.Fill(_innerData.OccupancyRate, 0);
        _innerMap = OccupancyGrid2DMap.Build_IOccupancyGridMap2DData(_innerData);
        _innerMap.TopZ = _innerData.TopZ;
        _innerMap.ButtonZ = _innerData.BottomZ;
        OccupancyHighGrid2DMap? staticHigh   = null;
        var                     staticHighId = "";
        EventBus<OccupancyHighGrid2DMap>.Instance.Subscribe(_staticMapTopicName, m =>
        {
            staticHigh = m;
            staticHighId = (staticHigh ?? throw new Exception("No static map"))
                .OccupancyData.Header
                .Identifier;
        });
        EventBus<IPointCloud>.Instance.Subscribe(_pointCloudTopicName,
            pointCloud =>
            {
                if (staticHigh is null)
                    return;
                Array.Fill(_innerMap.DataChangeable.OccupancyRate, 0);
                _innerMap.DataChangeable.DataChangable.HeaderData.Identifier = _costMapId;
                var points = Tf.Cast(_pointCloudId, staticHighId, pointCloud.Points);
                CostMap.Infrastructure.Algorithm.GridMapInner.SelectPointsInHighMap(ref points, 0.4f, 0.15f,
                    staticHigh);
                CostMap.Infrastructure.Algorithm.OccupancyGridMapBuilder.UpdateRateFromPointCloud(
                    Tf.Cast(staticHighId, _costMapId, points),
                    Tf.Cast(_sensorId,    _costMapId, Vector3.Zero),
                    _innerMap
                );
                EventBus<IGridMap2DData>.Instance.Publish(_costMapTopicName, _innerMap.OccupancyData.GridData);
            });
        return this;
    }

    /// <summary>
    /// 在事件系统中注册所有的事件
    /// </summary>
    /// <remarks>
    /// <para>输入：</para>
    /// <para>PointCloudInterface ->  Kernel.DataInterfaces.Sensor.IPointCloud</para>
    /// </remarks>
    /// <returns></returns>
    public PointCloudTo2dMap BuildOccupancyMap()
    {
        _innerData.Data = new sbyte[_innerData.Width           * _innerData.Height];
        _innerData.OccupancyRate = new float [_innerData.Width * _innerData.Height];
        Array.Fill(_innerData.OccupancyRate, 0);
        _innerMap = OccupancyGrid2DMap.Build_IOccupancyGridMap2DData(_innerData);
        _innerMap.TopZ = _innerData.TopZ;
        _innerMap.ButtonZ = _innerData.BottomZ;
        EventBus<IPointCloud>.Instance.Subscribe(_pointCloudTopicName,
            pointCloud =>
            {
                Array.Fill(_innerMap.DataChangeable.OccupancyRate, 2);
                _innerMap.DataChangeable.DataChangable.HeaderData.Identifier = _costMapId;

                CostMap.Infrastructure.Algorithm.OccupancyGridMapBuilder.UpdateRateFromPointCloud(
                    Tf.Cast(_pointCloudId, _costMapId, pointCloud.Points),
                    Tf.Cast(_sensorId,     _costMapId, Vector3.Zero),
                    _innerMap
                );
                EventBus<IGridMap2DData>.Instance.Publish(_costMapTopicName, _innerMap.OccupancyData.GridData);
            });
        return this;
    }

    /// <summary>
    /// 在事件系统中注册所有的事件
    /// </summary>
    /// <remarks>
    /// <para>输入：</para>
    /// <para>PointCloudInterface ->  Kernel.DataInterfaces.Sensor.IPointCloud</para>
    /// </remarks>
    /// <returns></returns>
    public PointCloudTo2dMap BuildOccupancyHighMap()
    {
        _innerData.Data = new sbyte[_innerData.Width           * _innerData.Height];
        _innerData.OccupancyRate = new float [_innerData.Width * _innerData.Height];
        _inner25DMap = OccupancyHighGrid2DMap.Build_IOccupancyGridMap2DData(_innerData);
        _inner25DMap.TopZ = _innerData.TopZ;
        _inner25DMap.ButtonZ = _innerData.BottomZ;
        Array.Fill(_innerData.OccupancyRate, 0);
        Array.Fill(_inner25DMap.High,        float.MaxValue);

        var step = Vector3.UnitZ * 0.1f;

        EventBus<IPointCloud>.Instance.Subscribe(_pointCloudTopicName,
            pointCloud =>
            {
                _inner25DMap.DataChangeable.DataChangable.HeaderData.Identifier = _costMapId;

                CostMap.Infrastructure.Algorithm.OccupancyGridMapBuilder.UpdateHighRateWithPointCloud(
                    Tf.Cast(_pointCloudId, _costMapId, pointCloud.Points),
                    Tf.Cast(_sensorId,     _costMapId, Vector3.Zero),
                    Tf.Cast(_chassisId,    _costMapId, step),
                    _inner25DMap
                );
                EventBus<IGridMap2DData>.Instance.Publish(_costMapTopicName, _inner25DMap.OccupancyData.GridData);
                EventBus<OccupancyHighGrid2DMap>.Instance.Publish(_costMapTopicName, _inner25DMap);
            });
        return this;
    }

    public PointCloudTo2dMap SetInput_StaticMapTopicName(string topicName)
    {
        _staticMapTopicName = topicName;
        return this;
    }

    public PointCloudTo2dMap SetInput_PointCloudTopicName(string topicName)
    {
        _pointCloudTopicName = topicName;
        return this;
    }


    public PointCloudTo2dMap SetId_PointCloud(string id)
    {
        _pointCloudId = id;
        return this;
    }

    public PointCloudTo2dMap SetId_Sensor(string id)
    {
        _sensorId = id;
        return this;
    }

    public PointCloudTo2dMap SetId_Map(string id)
    {
        _costMapId = id;
        return this;
    }

    public PointCloudTo2dMap SetId_Chassis(string id)
    {
        _chassisId = id;
        return this;
    }

    public PointCloudTo2dMap SetOutput_CostMapTopicName(string topicName)
    {
        _costMapTopicName = topicName;
        return this;
    }

    /// <summary>
    /// 设置输出地图格式
    /// </summary>
    /// <param name="width">x栅格数量</param>
    /// <param name="height">y栅格数量</param>
    /// <param name="threshold">高于这个阈值视为障碍物</param>
    /// <param name="resolution">分辨率（每个格子代表实际长度）</param>
    /// <param name="lossFree">空闲增量</param>
    /// <param name="lossOccu">占据增量</param>
    /// <param name="bottomZ">低于这个高度的点不处理</param>
    /// <param name="topZ">高于这个高度的点不处理</param>
    public PointCloudTo2dMap SetOutput_DataStructure(uint  width      = 100,
                                                     uint  height     = 100,
                                                     sbyte threshold  = 70,
                                                     float resolution = 60f,
                                                     float lossFree   = 0.7f,
                                                     float lossOccu   = -0.9f,
                                                     float bottomZ    = 0.1f,
                                                     float topZ       = 0.2f)
    {
        _innerData.Width = width;
        _innerData.Height = height;
        _innerData.Threshold = threshold;
        _innerData.Resolution = resolution;
        _innerData.LossFree = lossFree;
        _innerData.LossOccu = lossOccu;
        _innerData.BottomZ = bottomZ;
        _innerData.TopZ = topZ;
        return this;
    }

#endregion
}