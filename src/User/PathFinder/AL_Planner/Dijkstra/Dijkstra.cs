using System.Numerics;
using AllianceDM.StdComponent;
using Newtonsoft.Json;

namespace AllianceDM.ALPlanner;
class Dijkstra : Component
{

    public string mapPath;

    private GlobalESDFMap costMap;
    private Transform2D sentry;
    private ALPlannerDecisionMaker decisionMaker;



    public List<Vector2> Path => _path;

    private int[] _pathArray;
    private List<Vector2> _path;
    private DijkstraMap _pathMap;
    private float[,] _map;

    private int lastFrom = -1;


    public override void Start()
    {
        var json = File.ReadAllText(DecisionMakerDef.ComponentsPath + mapPath);
        _pathMap = JsonConvert.DeserializeObject<DijkstraMap>(json);
    }


    public override void Update()
    {
        _path = [];
        int k = _pathMap.Points.Length;
        var Colored = new bool[k];
        _pathArray = new int[k];
        if (_map == null)
            _map = new float[k, k];
        Buffer.BlockCopy(_pathMap.Map, 0, _map, 0, _pathMap.Map.Length * sizeof(float));
        var tempMap = new float[k];
        var (x, y) = costMap.Vector2ToXY(sentry.position);
        if (x < 0 || x > _pathMap.Voronoi.GetLength(0) || y < 0 || y > _pathMap.Voronoi.GetLength(1))
            return;
        int From = _pathMap.Voronoi[x, y];
        if (costMap[x, y] <= 0)
            From = lastFrom;
        lastFrom = From;

        (x, y) = costMap.Vector2ToXY(decisionMaker.TargetPosition);
        int To = _pathMap.Voronoi[x, y];
        Colored[From] = true;
        for (int i = 0; i < k; i++)
        {
            _map[From, i] = _map[i, From] = i != From ? (sentry.position - _pathMap.Points[i]).Length() : 0;
            _map[To, i] = _map[i, To] = i != To ? (decisionMaker.TargetPosition - _pathMap.Points[i]).Length() : 0;
            if (_pathMap.Access[From, i])
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
                if (!_pathMap.Access[t, j])
                    continue;
                if (tempMap[j] > min + _map[t, j])
                {
                    tempMap[j] = min + _map[t, j];
                    _pathArray[j] = t;
                }
            }
        }
        var tmp = _pathArray[To];
        _path.Add(decisionMaker.TargetPosition);
        while (tmp != From)
        {
            _path.Add(_pathMap.Points[tmp]);
            tmp = _pathArray[tmp];
        }
        _path.Add(sentry.position);
        _path.Reverse();
        return;

    }
}