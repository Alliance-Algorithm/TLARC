using System.Numerics;
using AllianceDM.IO.ROS2Msgs.Geometry;
using AllianceDM.StdComponent;

namespace AllianceDM.ALPlanner;

class HybridAStar : Component
{
    private GlobalESDFMap costMap;
    private Transform2D sentry;
    private Transform2D target;

    public string beginSpeedTopicName = "/chassis/speed";
    public float maxSearchDistanceRatio = 1.2f;
    public float retentionRatio = 0.2f;

    public List<Vector2> Path => _path;

    private List<Vector2> _path;
    private bool[,] _closeGrid;
    private PriorityQueue<Node3, float> openList;

    private float _beginSpeedAngle;

    private Pose2D _beginSpeedReceiver;

    public override void Start()
    {
        _beginSpeedReceiver.Subscript(beginSpeedTopicName, data => { _beginSpeedAngle = data.Theta; });
    }

    public override void Update()
    {
        Search();
        DownSample();
    }

    private void DownSample()
    {
        if (_path == null)
            return;
        var k = 1 / retentionRatio;
        List<Vector2> newPath = new();
        for (double i = 0, li = -1; i < _path.Count; i += k)
        {
            var j = Math.Round(i);
            if (li == j)
                continue;
            newPath.Add(_path[(int)j]);
            li = Math.Round(i);
        }
        _path = newPath;
    }

    private bool Search()
    {
        var maxDistance = maxSearchDistanceRatio * (sentry.position - target.position).Length();
        _path = [];
        openList = new();
        _closeGrid = new bool[costMap.SizeX, costMap.SizeY];
        Node3 from = new(sentry.position, _beginSpeedAngle, null, 0);
        Node3 to = new(target.position, float.PositiveInfinity, null, 0);
        openList.Enqueue(from, 0);
        while (openList.Count > 0)
        {
            var current = openList.Dequeue();
            var xy = costMap.Vector2ToXY(current.Pos);
            if (_closeGrid[xy.x, xy.y])
                continue;
            _closeGrid[xy.x, xy.y] = true;
            if (current == to)
            {
                to.Parent = current.Parent;
                break;
            }
            var children = current.ChildrenGen();
            foreach (var child in children)
            {
                if (child.G > maxDistance)
                    continue;
                xy = costMap.Vector2ToXY(child.Pos);
                if (_closeGrid[xy.x, xy.y])
                    continue;
                if (costMap[xy.x, xy.y] <= 0)
                    continue;
                openList.Enqueue(child, child.CalcF(to, costMap));
            }
        }

        if (to.Parent is null)
            return false;
        else
        {
            var k = to;
            _path.Add(to.Pos);
            while (k.Parent is not null)
            {
                k = k.Parent;
                _path.Add(k.Pos);
            }
            _path.Reverse();
            return true;
        }
    }


}