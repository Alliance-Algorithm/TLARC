using System.Numerics;
using AllianceDM.StdComponent;
using Newtonsoft.Json;

namespace AllianceDM.AlPlanner;
class Dijkstra : Component
{

    public string mapPath;

    private GlobalESDFMap costMap;
    private Transform2D sentry;
    private Transform2D target;

    public Vector2[] Points => pathMap.Points;
    public int[,] Voronoi => pathMap.Voronoi;
    public bool[,] Access => pathMap.Access;
    public float[,] Map;

    public List<Vector2> Path => _path;

    private int[] _pathArray;
    private List<Vector2> _path;
    private DijkstraMap pathMap;

    public override void Start()
    {
        pathMap = JsonConvert.DeserializeObject<DijkstraMap>(mapPath);
    }


    public override void Update()
    {
        _path = [];
        int k = Points.Length;
        var Colord = new bool[k];
        _pathArray = new int[k];
        if (Map == null)
            Map = new float[k, k];
        Buffer.BlockCopy(pathMap.Map, 0, Map, 0, pathMap.Map.Length * sizeof(float));
        var SMap = new float[k];
        var (x, y) = costMap.Vector2ToXY(sentry.position);
        int From = Voronoi[x, y];
        (x, y) = costMap.Vector2ToXY(target.position);
        int To = Voronoi[x, y];
        Colord[From] = true;
        for (int i = 0; i < k; i++)
        {
            Map[From, i] = Map[i, From] = i != From ? (sentry.position - Points[i]).Length() : 0;
            Map[To, i] = Map[i, To] = i != To ? (target.position - Points[i]).Length() : 0;
            if (Access[From, i])
            {
                _pathArray[i] = From;
                SMap[i] = Map[From, i];
            }
            else SMap[i] = float.MaxValue;
        }
        while (!Colord[To])
        {
            int t = -1;
            float min = float.MaxValue;
            for (int j = 0; j < k; j++)
            {
                if (Colord[j])
                    continue;
                if (min > SMap[j])
                {
                    min = SMap[j];
                    t = j;
                }
            }
            if (t == -1)
            {
                return;
            }
            Colord[t] = true;
            for (int j = 0; j < k; j++)
            {
                if (Colord[j])
                    continue;
                if (!Access[t, j])
                    continue;
                if (SMap[j] > min + Map[t, j])
                {
                    SMap[j] = min + Map[t, j];
                    _pathArray[j] = t;
                }
            }
        }
        var tmp = _pathArray[To];
        _path.Add(target.position);
        while (tmp != From)
        {
            _path.Add(Points[tmp]);
            tmp = _pathArray[tmp];
        }
        _path.Add(sentry.position);
        _path.Reverse();
        return;

    }
}