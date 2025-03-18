using System.Numerics;
using ALPlanner.Interfaces.ROS;
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
    private bool[,] _colored;

    private IO.ROS2Msgs.Nav.OccupancyGrid _dynamicMapReceiver;
    private IO.ROS2Msgs.Nav.OccupancyGrid _esdfPublisher;

    public sbyte this[int x, int y]
    {
        get => _map == null ? sbyte.MinValue :
         (x < 0 || y < 0 || x >= SizeX || y >= SizeY ? sbyte.MinValue : _map[x, y]);
        set => _map[x, y] = value;
    }

    public Vector3d XY2Vector3d(int x, int y) => new(
        x * Resolution - (SizeX - 1) * Resolution / 2.0f,
        y * Resolution - (SizeY - 1) * Resolution / 2.0f, 0);
    public (int x, int y) Vector3dToXY(Vector3d pos) => new(
        (int)Math.Round((pos.x + ((SizeX - 1) * Resolution / 2.0f)) / Resolution),
        (int)Math.Round((pos.y + ((SizeY - 1) * Resolution / 2.0f)) / Resolution));

    public override void Start()
    {
        if (mapPath == "")
        {
            _staticMap = new() { _resolution = 0.2f, _size_x = 70, _size_y = 70, _staticObs = new int[70, 70, 0], _staticMap = new sbyte[70, 70] };
        }
        else
        {
            var json = File.ReadAllText(TlarcSystem.RootPath + mapPath);
            _staticMap = JsonConvert.DeserializeObject<ESDFMapData>(json);
        }
        _dynamicMapReceiver = new(IOManager);
        _dynamicMapReceiver.Subscript(dynamicMapTopicName, data =>
        {
            DynamicMapData dy_data = new();
            var _position = data.Position;
            var _angle = data.angle;

            var tmp = Vector3dToXY(_sentry_position + Quaterniond.AxisAngleR(Vector3d.AxisZ, sentry.AngleR + Math.PI) * _position);

            dy_data._map = data.Map;
            dy_data.offsetX = tmp.x;
            dy_data.offsetY = tmp.y;
            dy_data.forward = Math.SinCos(sentry.AngleR + Math.PI);

            _dynamicMaps.Enqueue(dy_data);
            if (_dynamicMaps.Count > max_queue_length)
                _dynamicMaps.Dequeue();
        });
        _map = new sbyte[_staticMap.Map.GetLength(0), _staticMap.Map.GetLength(1)];
        _obstacles = new int[_staticMap.Obstacles.GetLength(0), _staticMap.Obstacles.GetLength(1), _staticMap.Obstacles.GetLength(2)];
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
        Buffer.BlockCopy(_staticMap.Map, 0, _map, 0, _map.Length * sizeof(sbyte));
        Buffer.BlockCopy(_staticMap.Obstacles, 0, _obstacles, 0, _obstacles.Length * sizeof(int));
        _colored = new bool[SizeX, SizeY];

        Queue<(int x, int y)> openList = new Queue<(int x, int y)>();
        foreach (var _dynamicMap in _dynamicMaps)
        {
            for (int i = 0, k = _dynamicMap._map.GetLength(0); i < k; i++)
                for (int j = 0; j < k; j++)
                {
                    var k_2 = k / 2;
                    var x = (int)Math.Round((i) * _dynamicMap.forward.Cos - (j) * _dynamicMap.forward.Sin + _dynamicMap.offsetX);
                    var y = (int)Math.Round((j) * _dynamicMap.forward.Cos + (i) * _dynamicMap.forward.Sin + _dynamicMap.offsetY);
                    if (x < 0 || x >= SizeX)
                        continue;
                    if (y < 0 || y >= SizeY)
                        continue;
                    if (_dynamicMap._map[i, j] == 0 && _staticMap.Map[x, y] != 0)
                        continue;
                    _map[x, y] = 0;
                    _obstacles[x, y, 0] = x;
                    _obstacles[x, y, 1] = y;
                    _colored[x, y] = true;
                    openList.Enqueue(new(x, y));
                }
        }
        while (openList.Count > 0)
        {
            var c = openList.Dequeue();
            for (int i = -1; i <= 1; i++)
                for (int j = -1; j <= 1; j++)
                {
                    var ci = c.x + i;
                    var cj = c.y + j;
                    if (i == 0 && j == 0)
                        continue;
                    if (ci < 0 || ci >= SizeX)
                        continue;
                    if (cj < 0 || cj >= SizeY)
                        continue;

                    // for calculate
                    var cObstacleX = _obstacles[ci, cj, 0];
                    var cObstacleY = _obstacles[ci, cj, 1];
                    if (cObstacleX != -1)
                    {
                        var m = (sbyte)(Math.Clamp(Math.Round(Math.Sqrt(Math.Pow(cObstacleX - c.x, 2) + Math.Pow(cObstacleY - c.y, 2))) * Resolution / maxDistance, 0, 1) * 100);
                        if (_map[c.x, c.y] > m)
                        {
                            _map[c.x, c.y] = m;
                            _obstacles[c.x, c.y, 0] = cObstacleX;
                            _obstacles[c.x, c.y, 1] = cObstacleY;
                        }

                    }
                    // for enqueue
                    if (_colored[ci, cj])
                        continue;
                    if (_obstacles[ci, cj, 0] == ci && _obstacles[ci, cj, 0] == cj)
                        continue;
                    _colored[ci, cj] = true;
                    openList.Enqueue((ci, cj));
                }
        }
        if (debug)
            _esdfPublisher.Publish((_map, Resolution, (uint)SizeX, (uint)SizeY));
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

    public Vector3d IndexToPositionInWorld(Vector3i position) => new(
        position.x * Resolution - (SizeX - 1) * Resolution / 2.0f,
        position.y * Resolution - (SizeY - 1) * Resolution / 2.0f, 0);

    public Vector3i PositionInWorldToIndex(Vector3d position) => new(
        (int)Math.Round((position.x + ((SizeX - 1) * Resolution / 2.0f)) / Resolution),
        (int)Math.Round((position.y + ((SizeY - 1) * Resolution / 2.0f)) / Resolution), 0);


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
}