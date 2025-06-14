using ALPlanner.Interfaces.ROS;
using Emgu.CV;
using Maps.Interfaces;
using TlarcKernel.IO.ProcessCommunicateInterfaces;

namespace Maps;

class OccupancyMapSubscript : Component, IGridMap, ISafeCorridorGenerator
{
  OccupancyMapData? _data = null;
  OccupancyMapData _data_init;
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
    return _data is null ? Vector3d.Zero : ((IGridMap)_data).IndexToPositionInWorld(position) - offset;
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
    _data_init = OccupancyGridMapHelper.Init(MapPath);
  }

  public override void Update()
  {
    using var a = _mapSubscript.Rent;
    if (_data is not null)
      _data.Dispose();
    _data = a?.Instance?.Value.Copy();
  }

  public bool CheckAccessibility(Vector3d index, float value = 0) =>
  value == 0 ? _data is null || ((IMap)_data).CheckAccessibility(index + offset, value) : ((IMap)_data_init).CheckAccessibility(index + offset, value);


  public SafeCorridorData Generate(Vector3d[] pointList, double maxLength = 2)
  {
    const double iterator = 0.3d;
    SafeCorridorData rectangles = new();
    if (_data is null || pointList.Length < 2) return rectangles;
    rectangles.PushIn(new(pointList[0].x, pointList[0].y, pointList[0].x, pointList[0].y, Matrix.Identity(2)));
    for (int i = 1; i < pointList.Length; i++)
    {
      var origin = ((pointList[i] + pointList[i - 1]) / 2).xy;
      var XDir = (pointList[i] - pointList[i - 1]).xy.Normalized;
      var YDir = new Vector2d(-XDir.y, XDir.x);
      var angle = -Math.Atan2(XDir.y, XDir.x);
      double LengthX = (pointList[i] - pointList[i - 1]).Length  ;
      double LengthY = _data.Resolution / 4;

      double[,] rotation = Matrix.Identity(2);

      var (sin, cos) = Math.SinCos(angle);
      rotation[0, 0] = cos;
      rotation[1, 0] = sin;
      rotation[0, 1] = -sin;
      rotation[1, 1] = cos;
      

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
              var tmp = LengthY + iterator;
              for (double k = 0; k < LengthX; k += iterator)
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
                origin += iterator / 2 * YDir;
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
              var tmp = LengthX + iterator;
              for (double k = 0; k < LengthY; k += iterator)
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
                origin += iterator / 2 * XDir;
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
              var tmp = LengthY + iterator;
              for (double k = 0; k < LengthX; k += iterator)
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
                origin -= iterator / 2 * YDir;
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
              var tmp = LengthX + iterator;
              for (double k = 0; k < LengthY; k += iterator)
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
                origin -= iterator / 2 * XDir;
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
      var max = origin + XDir * (LengthX / 2) + YDir * LengthY / 2;
      var min = origin - XDir * (LengthX / 2 ) - YDir * LengthY / 2;
      rectangles.PushIn(new(min.x, min.y, max.x, max.y, rotation));
    }
    rectangles.PushIn(new(pointList[^1].x, pointList[^1].y, pointList[^1].x, pointList[^1].y, Matrix.Identity(2)));
    return rectangles;
  }
}