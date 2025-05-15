using Accord;
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

        var tmp = PositionInWorldToIndex(_sentry_status.position +
             Quaterniond.AxisAngleR(Vector3d.AxisZ, _sentry_status.AngleR) * _position);

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
          var x = (int)(i * _dynamicMap.forward.Cos - j * _dynamicMap.forward.Sin + _dynamicMap.offsetX);
          var y = (int)(j * _dynamicMap.forward.Cos + i * _dynamicMap.forward.Sin + _dynamicMap.offsetY);
          if (_data_init[x, y] != 0)
            _data.Update(x, y, _dynamicMap._map[i, j]);
        }
    }
    _rosMapPublisher.Publish((_data.ToArray, _data.Resolution, (uint)_data.SizeX, (uint)_data.SizeY));
    // var cp = _data.Copy();
    // _mapPublisher.LoadInstance(ref cp);
  }

  public bool CheckAccessibility(Vector3d index, float value = 0)
  {
    return ((IMap)_data).CheckAccessibility(index + offset, value) || (value != 0 && !((IMap)_data_init).CheckAccessibility(index + offset, value));
  }

  public SafeCorridorData Generate(Vector3d[] pointList, double maxLength = 2)
  {
    SafeCorridorData rectangles = new();
    if (_data is null || pointList.Length < 2) return rectangles;
    rectangles.PushIn(new(pointList[0].x, pointList[0].y, pointList[0].x, pointList[0].y, Matrix.Identity(2)));
    for (int i = 1; i < pointList.Length; i++)
    {
      var origin = ((pointList[i] + pointList[i - 1]) / 2).xy;
      var XDir = (pointList[i] - pointList[i - 1]).xy.Normalized;
      var YDir = new Vector2d(-XDir.y, XDir.x);
      var angle = -Math.Atan2(XDir.y, XDir.x);
      double LengthX = Math.Round((pointList[i] - pointList[i - 1]).Length / _data.Resolution, MidpointRounding.AwayFromZero) * _data.Resolution;
      double LengthY = _data.Resolution;

      double[,] rotation = Matrix.Identity(2);

      rotation[0, 0] = Math.Cos(angle);
      rotation[1, 0] = Math.Sin(angle);
      rotation[0, 1] = -Math.Sin(angle);
      rotation[1, 1] = Math.Cos(angle);

      int flag = 0x0f;
      int j = 1;
      while (flag != 0)
      {
        switch (flag & j)
        {
          case 0b0001:
            {
              if (LengthY >= maxLength)
              {
                flag &= ~j;
                break;
              }
              bool check = true;
              var tmp = LengthY + _data.Resolution;
              for (double k = 0; k < LengthX; k += _data.Resolution)
              {
                var pos = origin + (k - LengthX / 2) * XDir + tmp / 2 * YDir;
                if (!CheckAccessibility(new Vector3d(pos.x, pos.y, 0)))
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
              if (LengthX >= maxLength)
              {
                flag &= ~j;
                break;
              }
              bool check = true;
              var tmp = LengthX + _data.Resolution;
              for (double k = 0; k < LengthY; k += _data.Resolution)
              {
                var pos = origin + (k - LengthY / 2) * YDir + tmp / 2 * XDir;
                if (!CheckAccessibility(new Vector3d(pos.x, pos.y, 0)))
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
                LengthX = tmp;
              }
              break;
            }
          case 0b0010:
            {
              if (LengthY >= maxLength)
              {
                flag &= ~j;
                break;
              }
              bool check = true;
              var tmp = LengthY + _data.Resolution;
              for (double k = 0; k < LengthX; k += _data.Resolution)
              {
                var pos = origin + (k - LengthX / 2) * XDir - tmp / 2 * YDir;
                if (!CheckAccessibility(new Vector3d(pos.x, pos.y, 0)))
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
              if (LengthX >= maxLength)
              {
                flag &= ~j;
                break;
              }
              bool check = true;
              var tmp = LengthX + _data.Resolution;
              for (double k = 0; k < LengthY; k += _data.Resolution)
              {
                var pos = origin + (k - LengthY / 2) * YDir - tmp / 2 * XDir;
                if (!CheckAccessibility(new Vector3d(pos.x, pos.y, 0)))
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
                LengthX = tmp;
              }
              break;
            }
          default: break;
        }

        j = (j << 1) % 0x0f;
      }
      // if (LengthX - 0.4 >= 0.3)
      //   LengthX -= 0.4;
      // LengthY *= 0.5;
      // LengthX *= 0.8;
      var max = origin + XDir * LengthX / 2 + YDir * LengthY / 2;
      var min = origin - XDir * LengthX / 2 - YDir * LengthY / 2;
      rectangles.PushIn(new(min.x, min.y, max.x, max.y, rotation));
    }
    rectangles.PushIn(new(pointList[^1].x, pointList[^1].y, pointList[^1].x, pointList[^1].y, Matrix.Identity(2)));
    return rectangles;
  }
}