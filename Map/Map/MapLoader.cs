using CostMap.Infrastructure.Algorithm;
using CostMap.Infrastructure.Data;
using Kernel.Core.EventBus;
using Kernel.Core.TransformTree;
using Kernel.DataInterfaces.Navigation;

namespace Map;

public class MapLoader
{
#region 成员变量与属性

    private string _mapPath = "";
    private IGridMap2DData? _map;
    private OccupancyHighGrid2DMap? _highMap;
    private string _mapEventName = "/map_server/static_map";

    private MapLoader(string mapPath, string mapEventName)
    {
        _mapEventName = mapEventName;
        _mapPath = mapPath;
    }

#endregion

#region 公共接口

    public MapLoader SetMapPath(string path)
    {
        _mapPath = path;
        return this;
    }

    public MapLoader SetEventName(string name)
    {
        _mapEventName = name;
        return this;
    }

    public MapLoader LoadMap()
    {
        _map = GridMapInner.LoadMap(_mapPath);
        return this;
    }

    public MapLoader LoadHighMap()
    {
        _highMap = GridMapInner.LoadHighMap(_mapPath);
        return this;
    }

    public MapLoader MapPublish()
    {
        EventBus<IGridMap2DData>.Instance.Publish(_mapEventName, _map ??= GridMapInner.LoadMap(_mapPath));
        return this;
    }

    public MapLoader HighMapPublish()
    {
        EventBus<OccupancyHighGrid2DMap>.Instance.Publish(_mapEventName,
            _highMap ??= GridMapInner.LoadHighMap(_mapPath));
        EventBus<IGridMap2DData>.Instance.Publish(_mapEventName, _highMap.OccupancyData.GridData);
        return this;
    }

    /// <summary>
    /// mapPath : ""
    /// <para/>
    /// mapEventName : "/map_server/static_map"
    /// <para/>
    /// mapLinkName : "/map_server/static_map"
    /// </summary>
    public static MapLoader Default => new("", "/map_server/static_map");

#endregion
}