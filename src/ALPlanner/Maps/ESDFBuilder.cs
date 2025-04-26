using Newtonsoft.Json;

namespace Maps;

static class ESDFBuilder
{
    private static sbyte[,] _colored;
    public static void Build(in ESDFMapData staticMap,
        in int sizeX, in int sizeY,
        in Queue<DynamicMapData> dynamicMaps,
        in int max_queue_length,
        float maxDistance,
        ref sbyte[,] _map,
        ref int[,,] _obstacles)
    {
        Buffer.BlockCopy(staticMap.Map, 0, _map, 0, _map.Length * sizeof(sbyte));
        Buffer.BlockCopy(staticMap.Obstacles, 0, _obstacles, 0, _obstacles.Length * sizeof(int));
        _colored = new sbyte[sizeX, sizeY];

        Queue<(int x, int y)> openList = new();
        foreach (var _dynamicMap in dynamicMaps)
        {
            for (int i = 0, k = _dynamicMap._map.GetLength(0); i < k; i++)
                for (int j = 0; j < k; j++)
                {
                    var k_2 = k / 2;
                    var x = (int)Math.Round((i) * _dynamicMap.forward.Cos - (j) * _dynamicMap.forward.Sin + _dynamicMap.offsetX);
                    var y = (int)Math.Round((j) * _dynamicMap.forward.Cos + (i) * _dynamicMap.forward.Sin + _dynamicMap.offsetY);
                    if (x < 0 || x >= sizeX)
                        continue;
                    if (y < 0 || y >= sizeY)
                        continue;
                    if (_dynamicMap._map[i, j] != 0 && staticMap.Map[x, y] <= 0)
                        continue;
                    if (_dynamicMap._map[i, j] == 0)
                        continue;

                    _colored[x, y] = (sbyte)((sbyte)(_colored[x, y] + (sbyte)(_map[x, y] / 4)) < _colored[x, y] ? sbyte.MaxValue : (_colored[x, y] + (sbyte)(_map[x, y] / 4)));
                    _colored[x, y] = (sbyte)((sbyte)(_colored[x, y] + (sbyte)(100 / max_queue_length + 1)) < _colored[x, y] ? sbyte.MaxValue : (_colored[x, y] + (sbyte)(100 / max_queue_length + 1)));
                    if (_colored[x, y] >= 100)
                    {
                        _map[x, y] = 0;
                        _obstacles[x, y, 0] = x;
                        _obstacles[x, y, 1] = y;
                        _colored[x, y] = 100;
                        openList.Enqueue(new(x, y));
                    }
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
                    if (ci < 0 || ci >= sizeX)
                        continue;
                    if (cj < 0 || cj >= sizeY)
                        continue;

                    // for calculate
                    var cObstacleX = _obstacles[ci, cj, 0];
                    var cObstacleY = _obstacles[ci, cj, 1];
                    if (cObstacleX != -1)
                    {
                        var m = (sbyte)(Math.Clamp(Math.Round(Math.Sqrt(Math.Pow(cObstacleX - c.x, 2) + Math.Pow(cObstacleY - c.y, 2))) * staticMap.Resolution / maxDistance, 0, 1) * 100);
                        if (_map[c.x, c.y] > m)
                        {
                            _map[c.x, c.y] = m;
                            _obstacles[c.x, c.y, 0] = cObstacleX;
                            _obstacles[c.x, c.y, 1] = cObstacleY;
                        }

                    }
                    // for enqueue
                    if (_colored[ci, cj] == 100)
                        continue;
                    if (_obstacles[ci, cj, 0] == ci && _obstacles[ci, cj, 0] == cj)
                        continue;
                    _colored[ci, cj] = 100;
                    openList.Enqueue((ci, cj));
                }
        }
    }
    public static void InitWithJson(ref ESDFMapData? originData, string jsonPath)
    {
        var json = File.ReadAllText(TlarcSystem.RootPath + jsonPath);
        originData = JsonConvert.DeserializeObject<ESDFMapData>(json);
    }
    public static void InitWithImage(ref ESDFMapData? originData, string imageImage)
    {
        throw new NotImplementedException();
    }
}