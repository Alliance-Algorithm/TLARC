using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using CostMap.Infrastructure.Data;
using Kernel.Core.EventBus;
using Kernel.Core.TransformTree;
using Kernel.Contract;
using Kernel.Contract.Navigation;
using Kernel.Contract.Sensor;

namespace Map;

public class PointCloudTo2dMap
{
    #region 类型定义

    private class InnerData 
    {
        public OGMData  OGM                                   = new();
        public float    BottomZ                 { get; set; } = 0.01f;
        public float    TopZ                    { get; set; } = 0.08f;
        public float    BlindCircleRadius       { get; set; } = 0.4f;
        public int      ForgetFrameCount        { get; set; } = 6;
        public float    RogMapSlidingThreshold  { get; set; } = 5;
        public float    InflationRadius         { get; set; } = 0.2f;
        public float    HighError               { get; set; } = 0.5f;
        public float    OccupyDensity           { get; set; } = 0.5f;
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
    /// <para>PointCloudInterface ->  Kernel.Contract.Sensor.IPointCloud</para>
    /// </remarks>
    /// <returns></returns>
    public PointCloudTo2dMap BuildOccupancyMapWithStaticHigh()
    {
        _innerData.OGM.GridMapData.Data     = new sbyte[_innerData.OGM.GridMapData.Width * _innerData.OGM.GridMapData.Height];
        _innerData.OGM.OccupancyRate        = new float[_innerData.OGM.GridMapData.Width * _innerData.OGM.GridMapData.Height];
        Array.Fill(_innerData.OGM.OccupancyRate, 0);
        _innerMap           = OccupancyGrid2DMap.Build_IOccupancyGridMap2DData(_innerData.OGM);
        _innerMap.TopZ      = _innerData.TopZ;
        _innerMap.ButtonZ   = _innerData.BottomZ;

        OccupancyHighGrid2DMap? staticHigh  = null;
        var staticHighId                    = "";
        
        EventBus<OccupancyHighGrid2DMap>.Instance.Subscribe(_staticMapTopicName, m =>
        {
            staticHigh = m;
            staticHighId = (staticHigh ?? throw new Exception("No static map"))
                .Data.GridMapData.Header.Identifier;
        });
        EventBus<PointCloud>.Instance.Subscribe(_pointCloudTopicName,
            pointCloud =>
            {
                if (staticHigh is null)
                    return;
                Array.Fill(_innerMap.Data.OccupancyRate, 0);
                _innerMap.Data.GridMapData.Header.Identifier = _costMapId;
                var arr = ArrayPool<Vector3>.Shared.Rent(pointCloud.Points.Length);
                var points = Tf.Cast(_pointCloudId, staticHighId, pointCloud.Points, arr);
                CostMap.Infrastructure.Algorithm.GridMapInner.SelectPointsInHighMap(ref points, 0.4f, 0.15f,
                    staticHigh);
                CostMap.Infrastructure.Algorithm.OccupancyGridMapBuilder.UpdateRateFromPointCloud(
                    Tf.Cast(staticHighId, _costMapId, points, points),
                    Tf.Cast(_sensorId, _costMapId, Vector3.Zero),
                    _innerMap
                );
                ArrayPool<Vector3>.Shared.Return(arr);
                EventBus<GridMap2DData>.Instance.Publish(_costMapTopicName, _innerMap.Data.GridMapData);
            });
        return this;
    }

    /// <summary>
    /// 在事件系统中注册所有的事件
    /// </summary>
    /// <remarks>
    /// <para>输入：</para>
    /// <para>PointCloudInterface ->  Kernel.Contract.Sensor.IPointCloud</para>
    /// </remarks>
    /// <returns></returns>
    public PointCloudTo2dMap BuildOccupancyMap()
    {
        _innerData.OGM.GridMapData.Data     = new sbyte[_innerData.OGM.GridMapData.Width * _innerData.OGM.GridMapData.Height];
        _innerData.OGM.OccupancyRate        = new float[_innerData.OGM.GridMapData.Width * _innerData.OGM.GridMapData.Height];
        Array.Fill(_innerData.OGM.OccupancyRate, 0);
        _innerMap           = OccupancyGrid2DMap.Build_IOccupancyGridMap2DData(_innerData.OGM);
        _innerMap.TopZ      = _innerData.TopZ;
        _innerMap.ButtonZ   = _innerData.BottomZ;
        EventBus<PointCloud>.Instance.Subscribe(_pointCloudTopicName,
            pointCloud =>
            {
                Array.Fill(_innerMap.Data.OccupancyRate, 2);
                _innerMap.Data.GridMapData.Header.Identifier = _costMapId;

                var arr = ArrayPool<Vector3>.Shared.Rent(pointCloud.Points.Length);
                CostMap.Infrastructure.Algorithm.OccupancyGridMapBuilder.UpdateRateFromPointCloud(
                    Tf.Cast(_pointCloudId, _costMapId, pointCloud.Points, arr),
                    Tf.Cast(_sensorId, _costMapId, Vector3.Zero),
                    _innerMap
                );
                ArrayPool<Vector3>.Shared.Return(arr);
                EventBus<GridMap2DData>.Instance.Publish(_costMapTopicName, _innerMap.Data.GridMapData);
            });
        return this;
    }

    /// <summary>
    /// 在事件系统中注册所有的事件
    /// </summary>
    /// <remarks>
    /// <para>输入：</para>
    /// <para>PointCloudInterface ->  Kernel.Contract.Sensor.IPointCloud</para>
    /// </remarks>
    /// <returns></returns>
    public PointCloudTo2dMap BuildOccupancyHighMap()
    {
        _innerData.OGM.GridMapData.Data     = new sbyte[_innerData.OGM.GridMapData.Width * _innerData.OGM.GridMapData.Height];
        _innerData.OGM.OccupancyRate        = new float[_innerData.OGM.GridMapData.Width * _innerData.OGM.GridMapData.Height];
        _inner25DMap = OccupancyHighGrid2DMap.Build_IOccupancyGridMap2DData(_innerData.OGM);
        _inner25DMap.TopZ = _innerData.TopZ;
        _inner25DMap.ButtonZ = _innerData.BottomZ;
        Array.Fill(_innerData.OGM.OccupancyRate, 0);
        Array.Fill(_inner25DMap.High, float.MaxValue);

        var step = Vector3.UnitZ * 0.1f;

        EventBus<PointCloud>.Instance.Subscribe(_pointCloudTopicName,
            pointCloud =>
            {
                _inner25DMap.Data.GridMapData.Header.Identifier = _costMapId;

                var arr = ArrayPool<Vector3>.Shared.Rent(pointCloud.Points.Length);
                CostMap.Infrastructure.Algorithm.OccupancyGridMapBuilder.UpdateHighRateWithPointCloud(
                    Tf.Cast(_pointCloudId, _costMapId, pointCloud.Points, arr),
                    Tf.Cast(_sensorId, _costMapId, Vector3.Zero),
                    Tf.Cast(_chassisId, _costMapId, step),
                    _inner25DMap
                );
                ArrayPool<Vector3>.Shared.Return(arr);
                EventBus<GridMap2DData>.Instance.Publish(_costMapTopicName, _inner25DMap.Data.GridMapData);
                EventBus<OccupancyHighGrid2DMap>.Instance.Publish(_costMapTopicName, _inner25DMap);
            });
        return this;
    }

    /// <summary>
    /// 在事件系统中注册所有的事件
    /// </summary>
    /// <remarks>
    /// <para>输入：</para>
    /// <para>PointCloudInterface ->  Kernel.Contract.Sensor.IPointCloud</para>
    /// </remarks>
    /// <returns></returns>
    public PointCloudTo2dMap BuildROGMap()
    {
        _innerROGMap = new ROGMap(
                        _innerData.OGM.GridMapData.Height, 
                        _innerData.OGM.GridMapData.Width, 
                        _innerData.InflationRadius, 
                        _innerData.OGM.GridMapData.Resolution, 
                        _innerData.TopZ, 
                        _innerData.BottomZ, 
                        _costMapId)
        {
            ForgetFrameCount = _innerData.ForgetFrameCount,
            SlidingThreshold = Math.Min(Math.Min(_innerData.OGM.GridMapData.Width, _innerData.OGM.GridMapData.Height) * _innerData.OGM.GridMapData.Resolution * 0.48f, _innerData.RogMapSlidingThreshold),
            BlindCircleRadius = _innerData.BlindCircleRadius,
            _lossHit = Math.Abs(_innerData.OGM.LossOccu),
            _lossMiss = -Math.Abs(_innerData.OGM.LossFree),
            HighError = _innerData.HighError,
            HighOccupyDensity = _innerData.OccupyDensity
        };


        EventBus<PointCloud>.Instance.Subscribe(_pointCloudTopicName,
            pointCloud =>
            {
                // var a = DateTime.UtcNow;
                var stamp = pointCloud.Header.Timestamp.ToStamp;
                CostMap.Infrastructure.Algorithm.ROGMap.MapSliding(_innerROGMap, Tf.Cast(_chassisId, _odomId, Vector3.Zero, stamp));

                Tf.SetTfNode(_costMapId, _innerROGMap.CenterInWorld, Quaternion.Identity , stamp);
                // var arr = new Vector3[pointCloud.Points.Length];
                // FUCK POOL
                var arr = ArrayPool<Vector3>.Shared.Rent(pointCloud.Points.Length);
                CostMap.Infrastructure.Algorithm.ROGMap.MapUpdate(
                                    _innerROGMap, 
                                    Tf.Cast(_sensorId,      _costMapId, Vector3.Zero,             stamp),
                                    Tf.Cast(_pointCloudId,  _costMapId, pointCloud.Points,   arr, stamp),
                                    pointCloud.Points.Length);
                ArrayPool<Vector3>.Shared.Return(arr);
                // Console.WriteLine((DateTime.UtcNow - a).TotalMilliseconds);
                // #warning 实际项目中不应该使用
                //                 var inflationMap = CostMap.Infrastructure.Algorithm.InflationLayerBuilder.Build(_innerROGMap);
                CostMap.Infrastructure.Algorithm.ROGMap.UpdateGridMap(_innerROGMap);
                EventBus<GridMap2DData>.Instance.Publish(_costMapTopicName, _innerROGMap.GridMap);

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
    public PointCloudTo2dMap SetOutput_Inflation(int radius)
    {
        CostMap.Infrastructure.Algorithm.InflationLayerBuilder.SetPara(radius);
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
        _innerData.OGM.GridMapData.Width = width;
        _innerData.OGM.GridMapData.Height = height;
        _innerData.OGM.GridMapData.Resolution = resolution;
        _innerData.OGM.Threshold = threshold;
        _innerData.OGM.LossFree = lossFree;
        _innerData.OGM.LossOccu = lossOccu;
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