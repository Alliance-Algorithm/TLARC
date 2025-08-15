using System.Numerics;
using System.Runtime.CompilerServices;
using CostMap.Infrastructure.Data;
using Kernel.Core.EventBus;
using Kernel.Core.Messages;
using Kernel.Core.TransformTree;
using Kernel.DataInterfaces.Navigation;

namespace Map;

using MapType = OccupancyHighGrid2DMap;

public class MapSaver
{
#region 局部变量

    private string _costMapTopicName;
    private string _saveMapTopicName;
    private string _saveTargetLink;
    private MapType? _data;

#endregion

    private MapSaver(string costMapTopicName, string saveMapTopicName, string saveTargetLink)
    {
        _costMapTopicName = costMapTopicName;
        _saveMapTopicName = saveMapTopicName;
        _saveTargetLink = saveTargetLink;
    }

# region 公共设置接口

    /// <summary>
    /// <para></para> pointCloudTopicName = "/tlarc/point_cloud/segment"
    /// <para></para> costMapTopicName = "/tlarc/map/cost_map_with_pcd"
    /// <para></para> uint  width      = 100,
    /// <para></para> uint  height     = 100,
    /// <para></para> sbyte threshold  = 70,
    /// <para></para> float resolution = 60f,
    /// <para></para> float lossFree   = 0.7f,
    /// <para></para> float lossOccu   = -0.9f,
    /// <para></para> float buttonZ    = -0.5f,
    /// <para></para> float topZ       = 0.1f
    /// </summary>
    public static MapSaver DefaultNew
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new("/tlarc/point_cloud/segment", "/tlarc/trigger/save_map", "car_init");
    }

    /// <summary>
    /// 在事件系统中注册所有的事件
    /// </summary>
    /// <remarks>
    /// <para>输入：</para>
    /// <para>GridMap2DData ->  Kernel.DataInterfaces.Navigation.IGridMap2DData</para>
    /// <para>SaveMap ->  Kernel.DataInterfaces.StringMessage</para>
    /// </remarks>
    public MapSaver Build()
    {
        EventBus<MapType>.Instance.Subscribe(_costMapTopicName, map => _data = map);
        EventBus<StringMessage>.Instance.Subscribe(_saveMapTopicName,
            str =>
            {
                if (_data is not null)
                {
                    var map = MapType.Build_Clone(_data);
                    map.DataChangeable.DataChangeable.HeaderData.Identifier = _saveTargetLink;
                    var xyz = Tf.Cast(_data.DataChangeable.DataChangeable.Header.Identifier, _saveTargetLink,
                        new Vector3(_data.DataChangeable.DataChangeable.Origin, 0));
                    map.DataChangeable.DataChangeable.Origin = new Vector2(xyz.X, xyz.Y);
                    CostMap.Infrastructure.Algorithm.GridMapInner.SaveHighMap(map, str.Instance);
                }
            });
        return this;
    }

    public MapSaver SetTrigger_SaveTriggerTopicName(string topicName)
    {
        _saveMapTopicName = topicName;
        return this;
    }

    public MapSaver SetInput_CostMapTopicName(string topicName)
    {
        _costMapTopicName = topicName;
        return this;
    }

#endregion
}