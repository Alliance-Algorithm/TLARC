using ALPlanner.Interfaces.ROS;
using Maps.Interfaces;
using TlarcKernel.IO.ProcessCommunicateInterfaces;

namespace Maps;

class OccupancyMapGenerator : Component, IGridMap, ISafeCorridorGenerator
{
  OccupancyMapData _data;
  OccupancyMapData _data_init;

  public Vector3i Index => ((IGridMap)_data).Index;
  public Vector3d OriginInWorld => ((IGridMap)_data).OriginInWorld;
  public Vector3i Size => ((IGridMap)_data).Size;

  public bool CheckAccessibility(Vector3d from, Vector3d to, float value = 0)
  {
    return ((IGridMap)_data).CheckAccessibility(from + offset, to + offset, value);
  }

  public bool CheckAccessibility(Vector3i index, float value = 0)
  {
    return ((IGridMap)_data).CheckAccessibility(index, value);
  }

  public Vector3d IndexToPositionInWorld(Vector3i position)
  {
    return ((IGridMap)_data).IndexToPositionInWorld(position);
  }

  public Vector3i PositionInWorldToIndex(Vector3d position)
  {
    return ((IGridMap)_data).PositionInWorldToIndex(position + offset);
  }

  ReadOnlyUnmanagedInterfacePublisher<OccupancyMapData> _mapPublisher = new("/occupancy/map");
  public int Mode = 0;
  public bool Debug = false;
  public string DynamicMapTopicName = "rmcs_map/map/grid";
  public string SaveMapPath = "./last.zip.tlm";
  public string MapPath = "";
  public Vector3d offset = Vector3d.Zero;
  private IO.ROS2Msgs.Nav.OccupancyGrid _dynamicMapReceiver;
  private IO.ROS2Msgs.Nav.OccupancyGrid _rosMapPublisher;
  private IO.ROS2Msgs.Std.Bool _safeMap;
  private PoseTransform2D sentry;
  private (Vector3d position, double AngleR) _sentry_status;
  public DynamicMapData? _dynamicMap = null;


  public override void Start()
  {
    _dynamicMapReceiver = new(IOManager);
    _dynamicMapReceiver.Subscript(
      DynamicMapTopicName,
      data =>
      {
        DynamicMapData dy_data = new();
        var _position = data.Position;
        var _angle = data.angle;

        var tmp = PositionInWorldToIndex(_sentry_status.position + _position);

        dy_data._map = data.Map;
        dy_data.offsetX = tmp.x;
        dy_data.offsetY = tmp.y;
        dy_data.forward = Math.SinCos(_sentry_status.AngleR);

        _dynamicMap = dy_data;
      }
    );
    _data = OccupancyGridMapHelper.Init(MapPath);
    _data_init = OccupancyGridMapHelper.Init(MapPath);
    if (Debug)
    {
      _rosMapPublisher = new(IOManager);
      _rosMapPublisher.RegistryPublisher("/tlarc/map");
    }

    _safeMap = new(IOManager);
    _safeMap.Subscript("/alplanner/save_map", _ => OccupancyGridMapHelper.Save(_data, SaveMapPath));
  }

  public override void Update()
  {
    _sentry_status = (sentry.Position, sentry.AngleR);
    if (_dynamicMap != null)
    {
      for (int i = 0, k = _dynamicMap._map.GetLength(0); i < k; i++)
        for (int j = 0; j < k; j++)
        {
          var x = (int)(i + _dynamicMap.offsetX);
          var y = (int)(j + _dynamicMap.offsetY);
          if (_data_init[x, y] != 0)
            _data.Update(x, y, _dynamicMap._map[i, j]);
        }
    }
    _rosMapPublisher.Publish((_data.ToArray, _data.Resolution, (uint)_data.SizeX, (uint)_data.SizeY));
    var cp = _data.Copy();
    _mapPublisher.LoadInstance(ref cp);
  }

  public bool CheckAccessibility(Vector3d index, float value = 0)
  {
    return ((IMap)_data).CheckAccessibility(index, value);
  }

  public SafeCorridorData Generate(Vector3d[] pointList, double maxLength) => throw new NotImplementedException();
}