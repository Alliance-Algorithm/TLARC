using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using CostMap.Infrastructure.Data;
using Kernel.Core.EventBus;
using Kernel.Core.TransformTree;
using Kernel.Contract;
using Kernel.Contract.Navigation;
using Kernel.Contract.Sensor;
using CostMap.Infrastructure.Algorithm;
using ROGMap = CostMap.Infrastructure.Data.ROGMap;

namespace Map;

public class PointCloudTo2dMap
{

    public static class ROGMapDefaultConfig
    {
        public const string EventPointCloudInputName    = "/tlarc/map_server/point_cloud";
        public const string EventRobotPositionName      = "/tlarc/map_server/sensor_pose";
        public const string EventGridMapName            = "/tlarc/map/cost_map_with_pcd";
        public const string EventTfName                 = "/tlarc/tf/publish";
        public const string EventPointCloudOutputName   = "/tlarc/point_cloud";
    
        public const string TfMapLink                   = "cost_map_link";
        public const string TfCarLink                   = "car_link";
        public const string TfOdomLink                  = "car_init";
        public const string TfLidarLink                 = "lidar_link";
        
        public const string PointCloudInputId           = TfLidarLink;
        public const string PointCloudCostMapSensorId   = TfLidarLink;
    }

    public static PointCloudTo2dMap ROGMapDefault => 
        new PointCloudTo2dMap()
        .SetInput_PointCloudTopicName(ROGMapDefaultConfig.EventPointCloudInputName)
        .SetOutput_DataStructure     (width: 600, height: 600, resolution: 0.02f, topZ: 0.7f, bottomZ: -0.1f, lossMiss: 0.7f, lossHit: -2.0f,
                                      blindCircleRadius: 0.4f, slidingThreshold: 1, forgetFrameCount: 15, highError: 0.1f, OccupyDensity: 0.3f, inflationRadius: 0.2f)
        .SetOutput_CostMapTopicName  (ROGMapDefaultConfig.EventGridMapName)
        .SetOutput_Inflation         (radius: 10)
        .SetId_Chassis               (ROGMapDefaultConfig.TfCarLink)
        .SetId_Sensor                (ROGMapDefaultConfig.PointCloudCostMapSensorId)
        .SetId_Odom                  (ROGMapDefaultConfig.TfOdomLink)
        .SetId_Map                   (ROGMapDefaultConfig.TfMapLink);

    #region 类型定义

    public class InnerData 
    {
        public OGMData  OGM                                   = new()
        {
            LHit      = 0.7f,
            LMiss     = -0.9f,
            Threshold = 70,
            GridMapData = new GridMap2DData
            {
                Header = new(){
                    Width      = 300,
                    Height     = 300,
                    Resolution = 0.02f,
                }
            }
        };
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

    private string _pointCloudTopicName = "/tlarc/sensor/lidar/point_cloud";
    private string _costMapTopicName = "/tlarc/map/cost_map";
    private string _staticMapTopicName = "/tlarc/map/static_high_map";

    private string _sensorId     = "lidar_link";
    private string _chassisId    = "car_link";
    private string _costMapId    = "cost_map_link";
    private string _odomId       = "odom";

    private OccupancyGrid2DMap? _innerMap;
    private OccupancyHighGrid2DMap? _inner25DMap;
    private ROGMap? _innerROGMap;

    public readonly InnerData MapData = new();

    #endregion

    #region 公共设置接口


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
        MapData.OGM.GridMapData.Data     = new sbyte[MapData.OGM.GridMapData.Header.Width * MapData.OGM.GridMapData.Header.Height];
        MapData.OGM.LG        = new float[MapData.OGM.GridMapData.Header.Width * MapData.OGM.GridMapData.Header.Height];
        Array.Fill(MapData.OGM.LG, 0);
        _innerMap           = OccupancyGrid2DMap.Build_IOccupancyGridMap2DData(MapData.OGM);
        _innerMap.TopZ      = MapData.TopZ;
        _innerMap.ButtonZ   = MapData.BottomZ;

        OccupancyHighGrid2DMap? staticHigh  = null;
        var staticHighId                    = "";
        
        EventBus<OccupancyHighGrid2DMap>.Instance.Subscribe(_staticMapTopicName, m =>
        {
            staticHigh = m;
            staticHighId = (staticHigh ?? throw new Exception("No static map"))
                .Data.GridMapData.Header.Header.Identifier;
        });
        EventBus<PointCloud>.Instance.Subscribe(_pointCloudTopicName,
            pointCloud =>
            {
                if (staticHigh is null)
                    return;
                Array.Fill(_innerMap.Data.LG, 0);
                _innerMap.Data.GridMapData.Header.Header.Identifier = _costMapId;
                var arr = ArrayPool<Vector3>.Shared.Rent(pointCloud.Points.Length);
                var points = Tf.Cast(pointCloud.Header.Identifier, staticHighId, pointCloud.Points, arr);
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
        MapData.OGM.GridMapData.Data     = new sbyte[MapData.OGM.GridMapData.Header.Width * MapData.OGM.GridMapData.Header.Height];
        MapData.OGM.LG        = new float[MapData.OGM.GridMapData.Header.Width * MapData.OGM.GridMapData.Header.Height];
        Array.Fill(MapData.OGM.LG, 0);
        _innerMap           = OccupancyGrid2DMap.Build_IOccupancyGridMap2DData(MapData.OGM);
        _innerMap.TopZ      = MapData.TopZ;
        _innerMap.ButtonZ   = MapData.BottomZ;
        EventBus<PointCloud>.Instance.Subscribe(_pointCloudTopicName,
            pointCloud =>
            {
                Array.Fill(_innerMap.Data.LG, 0);
                _innerMap.Data.GridMapData.Header.Header.Identifier = _costMapId;

                var arr = ArrayPool<Vector3>.Shared.Rent(pointCloud.Points.Length);
                CostMap.Infrastructure.Algorithm.OccupancyGridMapBuilder.UpdateRateFromPointCloud(
                    Tf.Cast(pointCloud.Header.Identifier, _costMapId, pointCloud.Points, arr),
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
        MapData.OGM.GridMapData.Data     = new sbyte[MapData.OGM.GridMapData.Header.Width * MapData.OGM.GridMapData.Header.Height];
        MapData.OGM.LG        = new float[MapData.OGM.GridMapData.Header.Width * MapData.OGM.GridMapData.Header.Height];
        _inner25DMap = OccupancyHighGrid2DMap.Build_IOccupancyGridMap2DData(MapData.OGM);
        _inner25DMap.TopZ = MapData.TopZ;
        _inner25DMap.ButtonZ = MapData.BottomZ;
        Array.Fill(MapData.OGM.LG, 0);
        Array.Fill(_inner25DMap.High, float.MaxValue);

        var step = Vector3.UnitZ * 0.1f;

        EventBus<PointCloud>.Instance.Subscribe(_pointCloudTopicName,
            pointCloud =>
            {
                _inner25DMap.Data.GridMapData.Header.Header.Identifier = _costMapId;

                var arr = ArrayPool<Vector3>.Shared.Rent(pointCloud.Points.Length);
                CostMap.Infrastructure.Algorithm.OccupancyGridMapBuilder.UpdateHighRateWithPointCloud(
                    Tf.Cast(pointCloud.Header.Identifier, _costMapId, pointCloud.Points, arr),
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
    public ROGMap BuildROGMap()
    {
        _innerROGMap = new ROGMap(
                        MapData.OGM.GridMapData.Header.Height, 
                        MapData.OGM.GridMapData.Header.Width, 
                        MapData.InflationRadius, 
                        MapData.OGM.GridMapData.Header.Resolution, 
                        MapData.TopZ, 
                        MapData.BottomZ, 
                        _costMapId)
        {
            ForgetFrameCount  = MapData.ForgetFrameCount,
            SlidingThreshold  = Math.Min(Math.Min(MapData.OGM.GridMapData.Header.Width, MapData.OGM.GridMapData.Header.Height) * MapData.OGM.GridMapData.Header.Resolution * 0.48f, MapData.RogMapSlidingThreshold),
            BlindCircleRadius = MapData.BlindCircleRadius,
            _lossHit  =  Math.Abs(MapData.OGM.LHit),
            _lossMiss = -Math.Abs(MapData.OGM.LMiss),
            HighError = MapData.HighError,
            HighOccupyDensity = MapData.OccupyDensity
        };

        EventBus<PointCloud>.Instance.Subscribe(_pointCloudTopicName,
            pointCloud =>
            {
                var stamp = pointCloud.Header.Timestamp.ToStamp;
                CostMap.Infrastructure.Algorithm.ROGMap.MapSliding(_innerROGMap, Tf.Cast(_chassisId, _odomId, Vector3.Zero, stamp));

                Tf.SetTfNode(_innerROGMap.Header.Identifier, _innerROGMap.CenterInWorld, Quaternion.Identity , stamp);
                // FUCK POOL
                var arr = ArrayPool<Vector3>.Shared.Rent(pointCloud.Points.Length);
                CostMap.Infrastructure.Algorithm.ROGMap.MapUpdate(
                                    _innerROGMap, 
                                    Tf.Cast(_sensorId,      _innerROGMap.Header.Identifier, Vector3.Zero,             stamp + 1),
                                    Tf.Cast(pointCloud.Header.Identifier,  _innerROGMap.Header.Identifier, pointCloud.Points,   arr, stamp + 1),
                                    pointCloud.Points.Length);
                ArrayPool<Vector3>.Shared.Return(arr);
                EventBus<ROGMap>.Instance.Publish(_costMapTopicName, _innerROGMap);
                if(_innerROGMap.Visualize)
                {
                    CostMap.Infrastructure.Algorithm.ROGMap.UpdateGridMap(_innerROGMap);
                    EventBus<GridMap2DData>.Instance.Publish(_costMapTopicName, _innerROGMap.GridMap);
                }
            });

        return _innerROGMap;
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
    /// <param name="lossMiss">空闲增量</param>
    /// <param name="lossHit">占据增量</param>
    /// <param name="bottomZ">低于这个高度的点不处理</param>
    /// <param name="topZ">高于这个高度的点不处理</param>
    /// <param name="forgetFrameCount">保留帧率</param>
    /// <param name="blindCircleRadius">盲区半径</param>
    /// <param name="slidingThreshold">局部地图滑动阈值</param>
    /// <param name="inflationRadius">局部地图不可行区域膨胀距离</param>
    public PointCloudTo2dMap SetOutput_DataStructure(uint  width                = 300,
                                                     uint  height               = 300,
                                                     sbyte threshold            = 70,
                                                     float resolution           = 0.02f,
                                                     float lossMiss             = 0.7f,
                                                     float lossHit              = -0.9f,
                                                     float bottomZ              = 0.1f,
                                                     float topZ                 = 0.2f,
                                                     int   forgetFrameCount     = 10,
                                                     float blindCircleRadius    = 0.4f,
                                                     float slidingThreshold     = 4,
                                                     float inflationRadius      = 0.2f,
                                                     float highError            = 0.5f,
                                                     float OccupyDensity        = 0.5f   )
    {
        MapData.OGM.GridMapData.Header.Width        = width == 300              ? MapData.OGM.GridMapData.Header.Width      : width;
        MapData.OGM.GridMapData.Header.Height       = height == 300             ? MapData.OGM.GridMapData.Header.Height     : height;
        MapData.OGM.GridMapData.Header.Resolution   = resolution == 0.02f       ? MapData.OGM.GridMapData.Header.Resolution : resolution;
        MapData.OGM.Threshold                       = threshold == 70           ? MapData.OGM.Threshold                     : threshold;
        MapData.OGM.LMiss                           = lossMiss == 0.7f          ? MapData.OGM.LMiss                         : lossMiss;
        MapData.OGM.LHit                            = lossHit == -0.9f          ? MapData.OGM.LHit                          : lossHit;
        MapData.BottomZ                             = bottomZ == 0.1f           ? MapData.BottomZ                           : bottomZ;
        MapData.TopZ                                = topZ == 0.2f              ? MapData.TopZ                              : topZ;
        MapData.ForgetFrameCount                    = forgetFrameCount == 10    ? MapData.ForgetFrameCount                  : forgetFrameCount;
        MapData.BlindCircleRadius                   = blindCircleRadius == 0.4f ? MapData.BlindCircleRadius                 : blindCircleRadius;
        MapData.RogMapSlidingThreshold              = slidingThreshold == 4     ? MapData.RogMapSlidingThreshold            : slidingThreshold;
        MapData.InflationRadius                     = inflationRadius == 0.2f   ? MapData.InflationRadius                   : inflationRadius;
        MapData.HighError                           = highError == 0.5f         ? MapData.HighError                         : highError;
        MapData.OccupyDensity                       = OccupyDensity == 0.5f     ? MapData.OccupyDensity                     : OccupyDensity;
        return this;
    }

    #endregion
}