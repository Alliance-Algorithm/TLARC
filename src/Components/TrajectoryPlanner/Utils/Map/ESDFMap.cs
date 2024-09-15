using System.Numerics;
using System.Text;
using Tlarc.IO.ROS2Msgs.Nav;
using Tlarc.StdComponent;

namespace Tlarc.TrajectoryPlanner.Utils;

class ESDFMap : GridMap
{
    private Transform2D sentry;

    public string mapPath;
    private string localOccupancyGridMapTopicName = "/map/local_map";
    private bool debug = false;

    private sbyte[,] _localOccupancyGridMap;
    private sbyte[,] _staticESDFMap;
    private sbyte[,] _realtimeESDFMap;
    private int[,,] _staticRootObstacle;
    private int[,,] _realtimeRootObstacle;
    private bool[,] _colored;
    private float _esdfCostMaxLengthInMeter = 0.1f;
    private Vector2 _mapOriginInWorld;
    private float _gridLength;
    private (int X, int Y) _size;
    private OccupancyGrid _localOccupancyGridMapReceiver;
    private OccupancyGrid _esdfPublisher;

    public override (int X, int Y) Size => _size;

    public override float this[Vector2 position]
    {
        get => Cost(position);
        set { }
    }

    public override void Start()
    {
        var rawData = File.ReadAllText(TlarcSystem.RootPath + mapPath, Encoding.ASCII);
        var rawDataInRow = rawData.Split('\n');
        var header = rawDataInRow[0].Split(' ');
        _mapOriginInWorld = new(float.Parse(header[0]), float.Parse(header[1]));
        _staticESDFMap = new sbyte[int.Parse(header[2]), int.Parse(header[3])];
        _realtimeESDFMap = new sbyte[int.Parse(header[2]), int.Parse(header[3])];
        _staticRootObstacle = new int[int.Parse(header[2]), int.Parse(header[3]), 2];
        _realtimeRootObstacle = new int[int.Parse(header[2]), int.Parse(header[3]), 2];
        _colored = new bool[int.Parse(header[2]), int.Parse(header[3])];
        _size = new(int.Parse(header[2]), int.Parse(header[3]));
        _gridLength = float.Parse(header[4]);
        for (int i = 1; i < rawDataInRow.Length; i++)
        {
            var dataRow = rawDataInRow[i].Split(' ');
            for (int j = 0; j < dataRow.Length; j++)
                _staticESDFMap[i, j] = sbyte.Parse(dataRow[j]);
        }
        _localOccupancyGridMapReceiver = new OccupancyGrid(IOManager);
        _localOccupancyGridMapReceiver.Subscript(localOccupancyGridMapTopicName, data => _localOccupancyGridMap = data.Map);
    }
    public override void Update()
    {
        var tmp = PositionToGridIndex(sentry.Position);
        int offsetX = tmp.x, offsetY = tmp.y;
        Buffer.BlockCopy(_staticESDFMap, 0, _realtimeESDFMap, 0, _staticESDFMap.Length * sizeof(sbyte));
        Buffer.BlockCopy(_staticRootObstacle, 0, _realtimeRootObstacle, 0, _staticRootObstacle.Length * sizeof(int));
        for (int i = 0; i < _size.X; i++)
            for (int j = 0; j < _size.Y; j++)
                _colored[i, j] = false;


        Queue<(int x, int y)> openList = new Queue<(int x, int y)>();

        var sentry_forward = Math.SinCos(sentry.angle);
        for (int i = 0, k = _localOccupancyGridMap.GetLength(0); i < k; i++)
            for (int j = 0; j < k; j++)
            {
                var k_2 = k / 2;
                var x = (int)Math.Round(-(i - k_2) * sentry_forward.Cos + (j - k_2) * sentry_forward.Sin + offsetX);
                var y = (int)Math.Round(-(j - k_2) * sentry_forward.Cos - (i - k_2) * sentry_forward.Sin + offsetY);
                if (x < 0 || x >= _size.X)
                    continue;
                if (y < 0 || y >= _size.Y)
                    continue;
                if (_localOccupancyGridMap[i, j] == 0 && _staticESDFMap[x, y] != 0)
                    continue;
                _realtimeESDFMap[x, y] = 0;
                _realtimeRootObstacle[x, y, 0] = x;
                _realtimeRootObstacle[x, y, 1] = y;
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
                    if (ci < 0 || ci >= _size.X)
                        continue;
                    if (cj < 0 || cj >= _size.Y)
                        continue;

                    // for calculate
                    var cObstacleX = _realtimeRootObstacle[ci, cj, 0];
                    var cObstacleY = _realtimeRootObstacle[ci, cj, 1];
                    if (cObstacleX != -1)
                    {
                        var m = (sbyte)(Math.Clamp(Math.Round(Math.Sqrt(Math.Pow(cObstacleX - c.x, 2) + Math.Pow(cObstacleY - c.y, 2))) * _gridLength / _esdfCostMaxLengthInMeter, 0, 1) * 100);
                        if (_realtimeESDFMap[c.x, c.y] > m)
                        {
                            _realtimeESDFMap[c.x, c.y] = m;
                            _realtimeRootObstacle[c.x, c.y, 0] = cObstacleX;
                            _realtimeRootObstacle[c.x, c.y, 1] = cObstacleY;
                        }

                    }
                    // for enqueue
                    if (_colored[ci, cj])
                        continue;
                    if (_realtimeRootObstacle[ci, cj, 0] == ci && _realtimeRootObstacle[ci, cj, 1] == cj)
                        continue;
                    _colored[ci, cj] = true;
                    openList.Enqueue((ci, cj));
                }
        }
        if (debug)
        {
            _realtimeESDFMap[tmp.x, tmp.y] = 120;
            _esdfPublisher.Publish((_realtimeESDFMap, _esdfCostMaxLengthInMeter, (uint)_size.X, (uint)_size.Y));
        }
    }
    public override (int x, int y) PositionToGridIndex(Vector2 position)
    {
        var positionFromOrigin = position + _mapOriginInWorld;
        positionFromOrigin /= _gridLength;
        return ((int)MathF.Round(positionFromOrigin.X), (int)MathF.Round(positionFromOrigin.Y));
    }

    public override Vector2 GridIndexToPosition(int x, int y)
    {
        Vector2 positionFromZero = new(x / _gridLength, y / _gridLength);
        return positionFromZero - _mapOriginInWorld;
    }
    public override float Cost(Vector2 position)
    {
        var (x, y) = PositionToGridIndex(position);
        if (x >= _realtimeESDFMap.GetLength(0) || y >= _realtimeESDFMap.GetLength(1) || x < 0 || y < 0)
            return UnReachable;
        return _realtimeESDFMap[x, y] / 100;
    }

}