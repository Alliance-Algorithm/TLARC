using System.Numerics;
using ALPlanner.Interfaces.ROS;
using Maps.Interfaces;
using Newtonsoft.Json;
using TlarcKernel;

namespace Maps;

public class ESDFGenerator : Component, IESDF
{
  public string mapPath = "";
  public string dynamicMapTopicName = "/map/local_map";
  public float maxDistance = 1;
  public bool debug = false;
  public int max_queue_length = 5;
  PoseTransform2D sentry;

  Vector3d offset = Vector3d.Zero;
  public int SizeX => _staticMap.SizeX;
  public int SizeY => _staticMap.SizeY;
  public float Resolution => _staticMap.Resolution;

  public Vector3i Index => throw new NotImplementedException();

  public Vector3d OriginInWorld => offset;
  private Vector3d _sentry_position;

  public Vector3i Size { get; set; }

  public double this[Vector3i index] => this[index.x, index.y];

  private ESDFMapData _staticMap;
  private sbyte[,] _map;
  Queue<DynamicMapData> _dynamicMaps;
  private int[,,] _obstacles;
  private IO.ROS2Msgs.Nav.OccupancyGrid _dynamicMapReceiver;
  private IO.ROS2Msgs.Nav.OccupancyGrid _esdfPublisher;

  public sbyte this[int x, int y]
  {
    get =>
      _map == null
        ? sbyte.MinValue
        : (x < 0 || y < 0 || x >= SizeX || y >= SizeY ? sbyte.MinValue : _map[x, y]);
    set => _map[x, y] = value;
  }

  public Vector3d XY2Vector3d(int x, int y) =>
    new(
      x * Resolution - (SizeX - 1) * Resolution / 2.0f,
      y * Resolution - (SizeY - 1) * Resolution / 2.0f,
      0
    );

  public (int x, int y) Vector3dToXY(Vector3d pos) =>
    new(
      (int)Math.Round((pos.x + ((SizeX - 1) * Resolution / 2.0f)) / Resolution),
      (int)Math.Round((pos.y + ((SizeY - 1) * Resolution / 2.0f)) / Resolution)
    );

  public override void Start()
  {
    if (mapPath.EndsWith(".json"))
      ESDFBuilder.InitWithJson(ref _staticMap, mapPath);
    else if (mapPath.EndsWith(".png"))
      ESDFBuilder.InitWithImage(ref _staticMap, mapPath);
    else
    {
      _staticMap = new()
      {
        _resolution = 0.1f,
        _size_x = 281,
        _size_y = 161,
        _staticObs = new int[281, 161, 2],
        _staticMap = new sbyte[281, 161],
      };
      for (int x = 0; x < SizeX; x++)
      for (int y = 0; y < SizeY; y++)
      {
        _staticMap._staticMap[x, y] = 100;
        _staticMap._staticObs[x, y, 0] = -1;
        _staticMap._staticObs[x, y, 1] = -1;
      }
    }
    _dynamicMapReceiver = new(IOManager);
    _dynamicMapReceiver.Subscript(
      dynamicMapTopicName,
      data =>
      {
        DynamicMapData dy_data = new();
        var _position = data.Position;
        var _angle = data.angle;

        var tmp = Vector3dToXY(
          _sentry_position
            + Quaterniond.AxisAngleR(Vector3d.AxisZ, sentry.AngleR + Math.PI) * _position
        );

        dy_data._map = data.Map;
        dy_data.offsetX = tmp.x;
        dy_data.offsetY = tmp.y;
        dy_data.forward = Math.SinCos(sentry.AngleR + Math.PI);

        _dynamicMaps.Enqueue(dy_data);
        if (_dynamicMaps.Count > max_queue_length)
          _dynamicMaps.Dequeue();
      }
    );
    _map = new sbyte[_staticMap.Map.GetLength(0), _staticMap.Map.GetLength(1)];
    _obstacles = new int[
      _staticMap.Obstacles.GetLength(0),
      _staticMap.Obstacles.GetLength(1),
      _staticMap.Obstacles.GetLength(2)
    ];
    _dynamicMaps = new();

    Size = new(SizeX, SizeY, 1);
    if (debug)
    {
      _esdfPublisher = new(IOManager);
      _esdfPublisher.RegistryPublisher("/tlarc/esdf");
    }
  }

  public override void Update()
  {
    _sentry_position = sentry.Position;
    ESDFBuilder.Build(
      _staticMap,
      SizeX,
      SizeY,
      _dynamicMaps,
      max_queue_length,
      maxDistance,
      ref _map,
      ref _obstacles
    );
    if (debug)
    {
      _esdfPublisher.Publish((_map, Resolution, (uint)SizeX, (uint)SizeY));
    }
  }

  public Vector3d Gradient(Vector3d position)
  {
    var index = PositionInWorldToIndex(position);
    var value = this[index.x, index.y];
    var valueXp = this[index.x + 1, index.y] - value;
    var valueXn = this[index.x - 1, index.y] - value;
    var valueYp = this[index.x, index.y + 1] - value;
    var valueYn = this[index.x, index.y + 1] - value;
    return new Vector3d(Math.Max(valueXn, valueXp), Math.Max(valueYn, valueYp), 0).Normalized;
  }

  public Vector3d IndexToPositionInWorld(Vector3i position) =>
    new(
      position.x * Resolution - (SizeX - 1) * Resolution / 2.0f,
      position.y * Resolution - (SizeY - 1) * Resolution / 2.0f,
      0
    );

  public Vector3i PositionInWorldToIndex(Vector3d position) =>
    new(
      (int)Math.Round((position.x + ((SizeX - 1) * Resolution / 2.0f)) / Resolution),
      (int)Math.Round((position.y + ((SizeY - 1) * Resolution / 2.0f)) / Resolution),
      0
    );

  public bool CheckAccessibility(Vector3i index, float value = 0)
  {
    return this[index.x, index.y] > value;
  }

  public bool CheckAccessibility(Vector3d from, Vector3d to, float value = 0)
  {
    bool ret = true;
    var err = to - from;
    var len = Math.Clamp(err.Length / Resolution, 0, double.MaxValue);
    err = err.Normalized;
    for (int i = 0; i < len; i++)
    {
      var index = PositionInWorldToIndex(from + err * Resolution * i);
      ret = this[index.x, index.y] > value;
      if (!ret)
        return ret;
    }
    var indexTail = PositionInWorldToIndex(to);
    return this[indexTail.x, indexTail.y] > value;
  }

  public bool CheckAccessibility(Vector3d index, float value = 0)
  {
    throw new NotImplementedException();
  }
}
