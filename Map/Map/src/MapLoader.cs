using CostMap.Infrastructure.Algorithm;
using CostMap.Infrastructure.Data;
using Kernel.Core.EventBus;
using Kernel.Core.TransformTree;
using Kernel.Contract.Navigation;

namespace Map;

public class MapLoader
{
    #region 成员变量与属性

    private string _mapPath = "";
    private GridMap2DData? _map;
    private OccupancyHighGrid2DMap? _highMap;
    public string MapEventName { get; private set; } = "/map_server/static_map";
    public string MapFrame => (_map ?? throw new Exception("No map loaded")).Header.Header.Identifier;

    private MapLoader(string mapPath, string mapEventName)
    {
        MapEventName = mapEventName;
        _mapPath = mapPath;
    }

    public uint MapHeight { get; private set; }
    public uint MapWidth { get; private set; }
    public float MapResolution { get; private set; }



    #endregion

    #region 公共接口

    public MapLoader SetMapPath(string path)
    {
        _mapPath = path;
        return this;
    }

    public MapLoader SetEventName(string name)
    {
        MapEventName = name;
        return this;
    }

    public MapLoader LoadMap()
    {
        _map            = GridMapInner.LoadMap(_mapPath);
        MapHeight       = _map.Value.Header.Height;
        MapWidth        = _map.Value.Header.Width;
        MapResolution   = _map.Value.Header.Resolution;
        return this;
    }
    public MapLoader LoadHighMap()
    {
        _highMap = GridMapInner.LoadHighMap(_mapPath);
        return this;
    }

    public MapLoader MapPublish()
    {
        EventBus<GridMap2DData>.Instance.Publish(MapEventName, _map ??= GridMapInner.LoadMap(_mapPath));
        return this;
    }

    public MapLoader HighMapPublish()
    {
        EventBus<OccupancyHighGrid2DMap>.Instance.Publish(MapEventName,
            _highMap ??= GridMapInner.LoadHighMap(_mapPath));
        EventBus<GridMap2DData>.Instance.Publish(MapEventName, _highMap.Data.GridMapData);
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