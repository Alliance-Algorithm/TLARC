using System.Runtime.CompilerServices;
using Kernel.Core.EventBus;
using Kernel.Core.Messages;
using Kernel.DataInterfaces.Navigation;

namespace Map;

public class MapSaver
{
#region 局部变量

    private string _costMapTopicName;
    private string _saveMapTopicName;
    private IGridMap2DData? _data;

#endregion

    private MapSaver(string costMapTopicName, string saveMapTopicName)
    {
        _costMapTopicName = costMapTopicName;
        _saveMapTopicName = saveMapTopicName;
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
        get => new("/tlarc/point_cloud/segment", "/tlarc/trigger/save_map");
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
        EventBus<IGridMap2DData>.Instance.Subscribe(_costMapTopicName, map => _data = map);
        EventBus<StringMessage>.Instance.Subscribe(_saveMapTopicName,
            str =>
            {
                if (_data is not null)
                    CostMap.Infrastructure.Algorithm.GridMapInner.SaveMap(_data, str.Instance);
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