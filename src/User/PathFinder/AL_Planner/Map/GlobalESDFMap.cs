using System.Numerics;
using AllianceDM.IO.ROS2Msgs.Nav;
using AllianceDM.StdComponent;
using Newtonsoft.Json;

namespace AllianceDM.ALPlanner;

public class GlobalESDFMap : Component
{
    private Transform2D sentry;

    public string MapPath;
    public string DynamicMapTopicName;
    public float MaxDistance;

    public int SizeX => _staticMap.SizeX;
    public int SizeY => _staticMap.SizeY;
    public float Resolution => _staticMap.Resolution;

    private ESDFMapData _staticMap;
    private sbyte[,] _map;
    private sbyte[,] _dynamicMap;
    private int[,,] _obstacles;
    private bool[,] _colored;

    private OccupancyGrid _dynamicMapReceiver;

    public sbyte this[int x, int y]
    {
        get => _map == null ? sbyte.MinValue :
         (x < 0 || y < 0 || x >= SizeX || y >= SizeY ? sbyte.MinValue : _map[x, y]);
        set => _map[x, y] = value;
    }

    public Vector2 XY2Vector2(int x, int y) => new Vector2(
        -y * Resolution + (SizeY - 1) * Resolution / 2.0f,
        -x * Resolution + (SizeX - 1) * Resolution / 2.0f);
    public (int x, int y) Vector2ToXY(Vector2 pos) => new(
        (int)Math.Round(-(pos.Y - ((SizeX - 1) * Resolution / 2.0f)) / Resolution),
        (int)Math.Round(-(pos.X - ((SizeY - 1) * Resolution / 2.0f)) / Resolution));

    public override void Start()
    {
        _staticMap = JsonConvert.DeserializeObject<ESDFMapData>(MapPath);
        _dynamicMapReceiver.Subscript(DynamicMapTopicName, data => _dynamicMap = data.Map);
    }

    public override void Update()
    {
        var tmp = Vector2ToXY(sentry.position);
        int offsetX = tmp.x, offsetY = tmp.y;

        Buffer.BlockCopy(_staticMap.Map, 0, _map, 0, _map.Length * sizeof(sbyte));
        Buffer.BlockCopy(_staticMap.Obstacles, 0, _obstacles, 0, _obstacles.Length * sizeof(int));
        _colored = new bool[SizeX, SizeY];

        Queue<(int x, int y)> openList = new Queue<(int x, int y)>();


        for (int i = 1, k = _dynamicMap.GetLength(0); i <= k; i++)
            for (int j = 1; j <= k; j++)
            {
                var x = -i + offsetX + k / 2;
                var y = -j + offsetY + k / 2;
                if (x < 0 || x >= SizeX)
                    continue;
                if (y < 0 || y >= SizeY)
                    continue;
                if (_dynamicMap[k - i, k - j] != 0 && _staticMap.Map[x, y] != 0)
                    continue;
                _map[x, y] = 0;
                _obstacles[x, y, 0] = x;
                _obstacles[x, y, 1] = y;
                _colored[x, y] = true;
                openList.Enqueue(new(x, y));
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
                        var m = (sbyte)(Math.Clamp(Math.Round(Math.Sqrt(Math.Pow(cObstacleX - c.x, 2) + Math.Pow(cObstacleY - c.y, 2))) * Resolution / MaxDistance, 0, 1) * 100);
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
    }
}
