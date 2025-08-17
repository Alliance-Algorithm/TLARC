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
    #region 类型定义

    private class InnerData : IOccupancyGridMap2DData, IHeader, IGridMap2DData
    {
        public IHeader Header => this;
        public IGridMap2DData GridMapData => this;
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
        public float BlindCircleRadius { get; set; } = 0.4f;
        public int ForgetFrameCount { get; set; } = 6;
        public float RogMapSlidingThreshold { get; set; } = 5;
        public float InflationRadius { get; set; } = 0.2f;
        public float HighError { get; set; } = 0.5f;
        public float OccupyDensity { get; set; } = 0.5f;
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
    private string _odomId = "odom";

    private OccupancyGrid2DMap? _innerMap;
    private OccupancyHighGrid2DMap? _inner25DMap;
    private ROGMap? _innerROGMap;

    private readonly InnerData _innerData;

    #endregion

    private PointCloudTo2dMap(string pointCloudTopicName, string costMapTopicName, string staticMapTopicName)
    {
        _pointCloudTopicName = pointCloudTopicName;
        _costMapTopicName = costMapTopicName;
        _staticMapTopicName = staticMapTopicName;
        _innerData = new InnerData();
    }

    #region 公共设置接口

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
        _innerData.Data = new sbyte[_innerData.Width * _innerData.Height];
        _innerData.OccupancyRate = new float[_innerData.Width * _innerData.Height];
        Array.Fill(_innerData.OccupancyRate, 0);
        _innerMap = OccupancyGrid2DMap.Build_IOccupancyGridMap2DData(_innerData);
        _innerMap.TopZ = _innerData.TopZ;
        _innerMap.ButtonZ = _innerData.BottomZ;
        OccupancyHighGrid2DMap? staticHigh = null;
        var staticHighId = "";
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
                _innerMap.DataChangeable.DataChangeable.HeaderData.Identifier = _costMapId;
                var points = Tf.Cast(_pointCloudId, staticHighId, pointCloud.Points);
                CostMap.Infrastructure.Algorithm.GridMapInner.SelectPointsInHighMap(ref points, 0.4f, 0.15f,
                    staticHigh);
                CostMap.Infrastructure.Algorithm.OccupancyGridMapBuilder.UpdateRateFromPointCloud(
                    Tf.Cast(staticHighId, _costMapId, points),
                    Tf.Cast(_sensorId, _costMapId, Vector3.Zero),
                    _innerMap
                );
                EventBus<IGridMap2DData>.Instance.Publish(_costMapTopicName, _innerMap.OccupancyData.GridMapData);
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
        _innerData.Data = new sbyte[_innerData.Width * _innerData.Height];
        _innerData.OccupancyRate = new float[_innerData.Width * _innerData.Height];
        Array.Fill(_innerData.OccupancyRate, 0);
        _innerMap = OccupancyGrid2DMap.Build_IOccupancyGridMap2DData(_innerData);
        _innerMap.TopZ = _innerData.TopZ;
        _innerMap.ButtonZ = _innerData.BottomZ;
        EventBus<IPointCloud>.Instance.Subscribe(_pointCloudTopicName,
            pointCloud =>
            {
                Array.Fill(_innerMap.DataChangeable.OccupancyRate, 2);
                _innerMap.DataChangeable.DataChangeable.HeaderData.Identifier = _costMapId;

                CostMap.Infrastructure.Algorithm.OccupancyGridMapBuilder.UpdateRateFromPointCloud(
                    Tf.Cast(_pointCloudId, _costMapId, pointCloud.Points),
                    Tf.Cast(_sensorId, _costMapId, Vector3.Zero),
                    _innerMap
                );
                EventBus<IGridMap2DData>.Instance.Publish(_costMapTopicName, _innerMap.OccupancyData.GridMapData);
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
        _innerData.Data = new sbyte[_innerData.Width * _innerData.Height];
        _innerData.OccupancyRate = new float[_innerData.Width * _innerData.Height];
        _inner25DMap = OccupancyHighGrid2DMap.Build_IOccupancyGridMap2DData(_innerData);
        _inner25DMap.TopZ = _innerData.TopZ;
        _inner25DMap.ButtonZ = _innerData.BottomZ;
        Array.Fill(_innerData.OccupancyRate, 0);
        Array.Fill(_inner25DMap.High, float.MaxValue);

        var step = Vector3.UnitZ * 0.1f;

        EventBus<IPointCloud>.Instance.Subscribe(_pointCloudTopicName,
            pointCloud =>
            {
                _inner25DMap.DataChangeable.DataChangeable.HeaderData.Identifier = _costMapId;

                CostMap.Infrastructure.Algorithm.OccupancyGridMapBuilder.UpdateHighRateWithPointCloud(
                    Tf.Cast(_pointCloudId, _costMapId, pointCloud.Points),
                    Tf.Cast(_sensorId, _costMapId, Vector3.Zero),
                    Tf.Cast(_chassisId, _costMapId, step),
                    _inner25DMap
                );
                EventBus<IGridMap2DData>.Instance.Publish(_costMapTopicName, _inner25DMap.OccupancyData.GridMapData);
                EventBus<OccupancyHighGrid2DMap>.Instance.Publish(_costMapTopicName, _inner25DMap);
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
    public PointCloudTo2dMap BuildROGMap()
    {
        _innerROGMap = new ROGMap(_innerData.Height, _innerData.Width, _innerData.InflationRadius, _innerData.Resolution, _innerData.TopZ, _innerData.BottomZ)
        {
            ForgetFrameCount = _innerData.ForgetFrameCount,
            SlidingThreshold = Math.Min(Math.Min(_innerData.Width, _innerData.Height) * _innerData.Resolution * 0.48f, _innerData.RogMapSlidingThreshold),
            BlindCircleRadius = _innerData.BlindCircleRadius,
            _lossHit = Math.Abs(_innerData.LossOccu),
            _lossMiss = -Math.Abs(_innerData.LossFree),
            Identifier = _costMapId,
            HighError = _innerData.HighError,
            HighOccupyDensity = _innerData.OccupyDensity
        };


        EventBus<IPointCloud>.Instance.Subscribe(_pointCloudTopicName,
            pointCloud =>
            {
                var a = DateTime.UtcNow;
                CostMap.Infrastructure.Algorithm.ROGMap.MapSliding(_innerROGMap, Tf.Cast(_chassisId, _odomId, Vector3.Zero));
                Tf.SetTfNode(_costMapId, _innerROGMap.CenterInWorld, Quaternion.Identity);
                CostMap.Infrastructure.Algorithm.ROGMap.MapUpdate(_innerROGMap, Tf.Cast(_chassisId, _costMapId, Vector3.Zero), Tf.Cast(_pointCloudId, _costMapId, pointCloud.Points));
                Console.WriteLine((DateTime.UtcNow - a).TotalMilliseconds);
                CostMap.Infrastructure.Algorithm.ROGMap.UpdateGridMap(_innerROGMap);
                EventBus<IGridMap2DData>.Instance.Publish(_costMapTopicName, _innerROGMap);
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
    public PointCloudTo2dMap SetId_Odom(string id)
    {
        _odomId = id;
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
    /// <param name="forgetFrameCount">保留帧率</param>
    /// <param name="blindCircleRadius">盲区半径</param>
    /// <param name="slidingThreshold">局部地图滑动阈值</param>
    /// <param name="inflationRadius">局部地图不可行区域膨胀距离</param>
    public PointCloudTo2dMap SetOutput_DataStructure(uint width = 300,
                                                     uint height = 300,
                                                     sbyte threshold = 70,
                                                     float resolution = 0.02f,
                                                     float lossFree = 0.7f,
                                                     float lossOccu = -0.9f,
                                                     float bottomZ = 0.1f,
                                                     float topZ = 0.2f,
                                                     int forgetFrameCount = 10,
                                                     float blindCircleRadius = 0.4f,
                                                     float slidingThreshold = 4,
                                                     float inflationRadius = 0.2f,
                                                     float highError = 0.5f,
                                                     float OccupyDensity = 0.5f)
    {
        _innerData.Width = width;
        _innerData.Height = height;
        _innerData.Threshold = threshold;
        _innerData.Resolution = resolution;
        _innerData.LossFree = lossFree;
        _innerData.LossOccu = lossOccu;
        _innerData.BottomZ = bottomZ;
        _innerData.TopZ = topZ;
        _innerData.ForgetFrameCount = forgetFrameCount;
        _innerData.BlindCircleRadius = blindCircleRadius;
        _innerData.RogMapSlidingThreshold = slidingThreshold;
        _innerData.InflationRadius = inflationRadius;
        _innerData.HighError = highError;
        _innerData.OccupyDensity = OccupyDensity;
        return this;
    }

    #endregion
}