using ALPlanner.Interfaces.ROS;
using Maps.Interfaces;
using TlarcKernel.IO.ProcessCommunicateInterfaces;

namespace Maps;

class OccupancyMapSubscript : Component, IGridMap, ISafeCorridorGenerator
{
  OccupancyMapData? _data = null;
  public string MapName = "/occupancy/map";
  ReadOnlyUnmanagedSubscription<OccupancyMapData> _mapSubscript;

  public Vector3i Index => _data is null ? Vector3i.Zero : ((IGridMap)_data).Index;
  public Vector3d OriginInWorld => _data is null ? Vector3d.Zero : ((IGridMap)_data).OriginInWorld;
  public Vector3i Size => _data is null ? Vector3i.Zero : ((IGridMap)_data).Size;

  public bool CheckAccessibility(Vector3d from, Vector3d to, float value = 0)
  {
    return _data is not null && ((IGridMap)_data).CheckAccessibility(from, to, value);
  }

  public bool CheckAccessibility(Vector3i index, float value = 0)
  {
    return _data is not null && ((IGridMap)_data).CheckAccessibility(index, value);
  }

  public Vector3d IndexToPositionInWorld(Vector3i position)
  {
    return _data is null ? Vector3d.Zero : ((IGridMap)_data).IndexToPositionInWorld(position);
  }

  public Vector3i PositionInWorldToIndex(Vector3d position)
  {
    return _data is null ? Vector3i.Zero : ((IGridMap)_data).PositionInWorldToIndex(position);
  }

  public int Mode = 0;
  public bool debug = false;
  public string dynamicMapTopicName = "/map/local_map";
  private IO.ROS2Msgs.Nav.OccupancyGrid _dynamicMapReceiver;
  private IO.ROS2Msgs.Nav.OccupancyGrid _esdfPublisher;
  private PoseTransform2D sentry;
  private (Vector3d position, double AngleR) _sentry_status;
  public DynamicMapData _dynamicMap;

  public override void Start()
  {
    _mapSubscript = new(MapName);
  }

  public override void Update()
  {
    _data = _mapSubscript.Rent?.Instance?.Value;
    if (_data is null)
      return;

    for (int i = 0, k = _dynamicMap._map.GetLength(0); i < k; i++)
      for (int j = 0; j < k; j++)
      {
        var x = (int)
          Math.Round(i * _dynamicMap.forward.Cos - j * _dynamicMap.forward.Sin + _dynamicMap.offsetX);
        var y = (int)
          Math.Round(j * _dynamicMap.forward.Cos + i * _dynamicMap.forward.Sin + _dynamicMap.offsetY);

        _data.Update(x, y, _dynamicMap._map[i, j]);
      }
  }

  public bool CheckAccessibility(Vector3d index, float value = 0) => _data is null || ((IMap)_data).CheckAccessibility(index, value);


  public SafeCorridorData Generate(Vector3d[] pointList)
  {
    SafeCorridorData rectangles = new();
    if (_data is null) return rectangles;
    for (int i = 0; i < pointList.Length; i++)
    {
      var tmp = PositionInWorldToIndex(pointList[i]);
      int XMin = tmp.x;
      int XMax = tmp.x;
      int YMin = tmp.y;
      int YMax = tmp.y;

      int flag = 0x0f;
      int j = 1;
      while (flag != 0 || (XMax - XMin) > (1 / _data.Resolution) || (YMax - YMin) > (1 / _data.Resolution))
      {
        if ((flag & j) == 0)
        {
          j++;
          continue;
        }
        List<Vector2i> indexes = [];
        switch (j)
        {
          case 0b0001:
            for (int k = YMin; k <= YMax; k++)
              indexes.Add(new(XMin - 1, k));
            if (indexes.All(i => CheckAccessibility(new Vector3d(i.x, i.y, 0))))
              XMin -= 1;
            else
              flag &= ~j;
            break;
          case 0b0010:
            for (int k = XMin; k <= XMax; k++)
              indexes.Add(new(k, YMin - 1));
            if (indexes.All(i => CheckAccessibility(new Vector3d(i.x, i.y, 0))))
              XMin -= 1;
            else
              flag &= ~j;
            break;
          case 0b0100:
            for (int k = YMin; k <= YMax; k++)
              indexes.Add(new(XMax + 1, k));
            if (indexes.All(i => CheckAccessibility(new Vector3d(i.x, i.y, 0))))
              XMin -= 1;
            else
              flag &= ~j;
            break;
          case 0b1000:
            for (int k = XMin; k <= YMax; k++)
              indexes.Add(new(k, YMax + 1));
            if (indexes.All(i => CheckAccessibility(new Vector3d(i.x, i.y, 0))))
              XMin -= 1;
            else
              flag &= ~j;
            break;
        }
        j = (j << 1) % 0x10;
      }
      rectangles.PushIn(new((XMax + XMin) * _data.Resolution / 2.0, (YMax + YMin) * _data.Resolution / 2.0, (XMax - XMin) * _data.Resolution, (YMax - YMin) * _data.Resolution));
    }
    return rectangles;
  }
}
