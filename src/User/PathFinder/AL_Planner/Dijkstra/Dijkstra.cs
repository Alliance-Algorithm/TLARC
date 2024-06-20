using System.Numerics;
using AllianceDM.StdComponent;
using Newtonsoft.Json;

namespace AllianceDM.ALPlanner;
class Dijkstra : Component
{

    public string mapPath;

    private GlobalESDFMap costMap;
    private Transform2D sentry;
    private Transform2D target;


    public List<Vector2> Path => _path;

    private int[] _pathArray;
    private List<Vector2> _path;
    private DijkstraMap pathMap;
    private float[,] _map;


    public override void Start()
    {
        pathMap = JsonConvert.DeserializeObject<DijkstraMap>(mapPath);
    }


    public override void Update()
    {
        _path = [];
        int k = pathMap.Points.Length;
        var Colored = new bool[k];
        _pathArray = new int[k];
        if (_map == null)
            _map = new float[k, k];
        Buffer.BlockCopy(pathMap.Map, 0, _map, 0, pathMap.Map.Length * sizeof(float));
        var tempMap = new float[k];
        var (x, y) = costMap.Vector2ToXY(sentry.position);
        int From = pathMap.Voronoi[x, y];
        (x, y) = costMap.Vector2ToXY(target.position);
        int To = pathMap.Voronoi[x, y];
        Colored[From] = true;
        for (int i = 0; i < k; i++)
        {
            _map[From, i] = _map[i, From] = i != From ? (sentry.position - pathMap.Points[i]).Length() : 0;
            _map[To, i] = _map[i, To] = i != To ? (target.position - pathMap.Points[i]).Length() : 0;
            if (pathMap.Access[From, i])
            {
                _pathArray[i] = From;
                tempMap[i] = _map[From, i];
            }
            else tempMap[i] = float.MaxValue;
        }
        while (!Colored[To])
        {
            int t = -1;
            float min = float.MaxValue;
            for (int j = 0; j < k; j++)
            {
                if (Colored[j])
                    continue;
                if (min > tempMap[j])
                {
                    min = tempMap[j];
                    t = j;
                }
            }
            if (t == -1)
            {
                return;
            }
            Colored[t] = true;
            for (int j = 0; j < k; j++)
            {
                if (Colored[j])
                    continue;
                if (!pathMap.Access[t, j])
                    continue;
                if (tempMap[j] > min + _map[t, j])
                {
                    tempMap[j] = min + _map[t, j];
                    _pathArray[j] = t;
                }
            }
        }
        var tmp = _pathArray[To];
        _path.Add(target.position);
        while (tmp != From)
        {
            _path.Add(pathMap.Points[tmp]);
            tmp = _pathArray[tmp];
        }
        _path.Add(sentry.position);
        _path.Reverse();
        return;

    }
}