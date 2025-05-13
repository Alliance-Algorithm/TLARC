using ALPlanner.Interfaces.ROS;
using Maps.Interfaces;
using TlarcKernel.IO.ProcessCommunicateInterfaces;

namespace Maps;

class OccupancyMapSubscript : Component, IGridMap, ISafeCorridorGenerator
{
  OccupancyMapData? _data = null;
  ReadOnlyUnmanagedSubscription<OccupancyMapData> _mapSubscript = new("/occupancy/map");

  public Vector3i Index => _data is null ? Vector3i.Zero : ((IGridMap)_data).Index;
  public Vector3d OriginInWorld => _data is null ? Vector3d.Zero : ((IGridMap)_data).OriginInWorld;
  public Vector3i Size => _data is null ? Vector3i.Zero : ((IGridMap)_data).Size;
  public Vector3d offset;

  public bool CheckAccessibility(Vector3d from, Vector3d to, float value = 0)
  {
    return _data is not null && ((IGridMap)_data).CheckAccessibility(from + offset, to + offset, value);
  }

  public bool CheckAccessibility(Vector3i index, float value = 0)
  {
    return _data is not null &&
    index.x < Size.x && index.y < Size.y &&
    index.x >= 0 && index.y >= 0
    && ((IGridMap)_data).CheckAccessibility(index, value);
  }

  public Vector3d IndexToPositionInWorld(Vector3i position)
  {
    return _data is null ? Vector3d.Zero : ((IGridMap)_data).IndexToPositionInWorld(position);
  }

  public Vector3i PositionInWorldToIndex(Vector3d position)
  {
    return _data is null ? Vector3i.Zero : ((IGridMap)_data).PositionInWorldToIndex(position + offset);
  }

  public int Mode = 0;
  public bool debug = false;
  public string dynamicMapTopicName = "/map/local_map";
  public string MapPath = "";

  public override void Start()
  {
    _data = OccupancyGridMapHelper.Init(MapPath);
  }

  public override void Update()
  {
    using var a = _mapSubscript.Rent;
    if (_data is not null)
      _data.Dispose();
    _data = a?.Instance?.Value.Copy();
  }

  public bool CheckAccessibility(Vector3d index, float value = 0) => _data is null || ((IMap)_data).CheckAccessibility(index, value);


  public SafeCorridorData Generate(Vector3d[] pointList, double maxLength = 2)
  {
    SafeCorridorData rectangles = new();
    if (_data is null) return rectangles;
    for (int i = 1; i < pointList.Length; i++)
    {
      var origin = ((pointList[i] + pointList[i - 1]) / 2).xy;
      var XDir = (pointList[i] - pointList[i - 1]).xy.Normalized;
      var YDir = new Vector2d(-XDir.y, XDir.x);
      var angle = -Math.Atan2(XDir.y, XDir.x);
      double LengthX = (pointList[i] - pointList[i - 1]).Length + _data.Resolution;
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
      var max = origin + XDir * LengthX / 2 + YDir * LengthY / 2;
      var min = origin - XDir * LengthX / 2 - YDir * LengthY / 2;
      rectangles.PushIn(new(min.x, min.y, max.x, max.y, rotation));
    }
    return rectangles;
  }
}
