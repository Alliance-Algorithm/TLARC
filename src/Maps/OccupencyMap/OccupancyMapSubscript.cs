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
    for (int i = 1; i < pointList.Length; i++)
    {
      var origin = ((pointList[i] + pointList[i - 1]) / 2).xy;
      var XDir = (pointList[i] - pointList[i - 1]).xy.Normalized;
      var YDir = new Vector2d(-XDir.y, XDir.x);
      var angle = -Math.Atan2(XDir.y, XDir.x);
      double LengthX = (pointList[i] - pointList[i - 1]).Length;
      double LengthY = _data.Resolution;

      double[,] rotation = Matrix.Identity(2);

      rotation[0, 0] = Math.Cos(angle);
      rotation[1, 0] = Math.Sin(angle);
      rotation[0, 1] = -Math.Sin(angle);
      rotation[0, 0] = Math.Cos(angle);

      int flag = 0x0f;
      int j = 1;

      while (flag != 0)
      {
        switch (flag & j)
        {
          case 0b0001:
            {
              bool check = true;
              var tmp = LengthY + _data.Resolution;
              for (double k = 0; k < LengthX; k += _data.Resolution)
              {
                var pos = origin + (k - LengthX / 2) * XDir + tmp / 2 * YDir;
                if (CheckAccessibility(new Vector3d(pos.x, pos.y, 0)))
                {
                  check = false;
                  break;
                }
              }
              if (!check)
                flag &= ~j;
              else
              {
                origin += _data.Resolution / 2 * YDir;
                LengthY = tmp;
              }
              break;
            }
          case 0b0100:
            {
              bool check = true;
              var tmp = LengthX + _data.Resolution;
              for (double k = 0; k < LengthY; k += _data.Resolution)
              {
                var pos = origin + (k - LengthY / 2) * YDir + tmp / 2 * XDir;
                if (CheckAccessibility(new Vector3d(pos.x, pos.y, 0)))
                {
                  check = false;
                  break;
                }
              }
              if (!check)
                flag &= ~j;
              else
              {
                origin += _data.Resolution / 2 * XDir;
                LengthY = tmp;
              }
              break;
            }
          case 0b0010:
            {
              bool check = true;
              var tmp = LengthY + _data.Resolution;
              for (double k = 0; k < LengthX; k += _data.Resolution)
              {
                var pos = origin + (k - LengthX / 2) * XDir - tmp / 2 * YDir;
                if (CheckAccessibility(new Vector3d(pos.x, pos.y, 0)))
                {
                  check = false;
                  break;
                }
              }
              if (!check)
                flag &= ~j;
              else
              {
                origin -= _data.Resolution / 2 * YDir;
                LengthY = tmp;
              }
              break;
            }
          case 0b1000:
            {
              bool check = true;
              var tmp = LengthX + _data.Resolution;
              for (double k = 0; k < LengthY; k += _data.Resolution)
              {
                var pos = origin + (k - LengthY / 2) * YDir - tmp / 2 * XDir;
                if (CheckAccessibility(new Vector3d(pos.x, pos.y, 0)))
                {
                  check = false;
                  break;
                }
              }
              if (!check)
                flag &= ~j;
              else
              {
                origin -= _data.Resolution / 2 * XDir;
                LengthY = tmp;
              }
              break;
            }
          default: break;
        }

        j = (j << 1) % 0x0f;
      }
      rectangles.PushIn(new(origin.x, origin.y, LengthX, LengthY, rotation));
    }
    return rectangles;
  }
}
