using System.Numerics;

namespace Tlarc.TrajectoryPlanner.Utils;

class HybridAStar : PathGenerator
{
    GridMap gridMap;

    public Vector2 AgentPosition => _agentPosition;

    public string beginSpeedTopicName = "/chassis/speed";
    private float sampleThreshold = 1.2f;

    private List<Vector3> _path;
    private bool[,] _closeGrid;
    private PriorityQueue<Node3, float> openList;

    private float _beginSpeedAngle = float.PositiveInfinity;

    private Vector2 _agentPosition = new();
    public bool Found { get; set; }
    public override List<Vector3> Path => _path;

    private void DownSample()
    {
        List<Vector3> pathNew = new();
        float sampleVal = 0;
        foreach (var k in _path)
        {
            sampleVal += sampleThreshold - gridMap.Cost(new(k.X, k.Y));
            if (sampleVal > 1)
            {
                sampleVal = 0;
                pathNew.Add(k);
            }
        }
        _path = pathNew;
    }

    public override void Start()
    {
        _closeGrid = new bool[gridMap.Size.X, gridMap.Size.Y];
    }

    public override bool Search(Vector2 target, out List<Vector3> path)
    {
        _path = [];
        openList = new();
        Node3 from = new(_agentPosition, _beginSpeedAngle, null, 0);
        Node3 to = new(target, float.PositiveInfinity, null, 0);
        openList.Enqueue(from, 0);
        while (openList.Count > 0)
        {
            var current = openList.Dequeue();
            var xy = gridMap.PositionToGridIndex(current.Pos);
            if (_closeGrid[xy.x, xy.y])
                continue;
            _closeGrid[xy.x, xy.y] = true;
            if (current == to)
            {
                to.Parent = current.Parent;
                break;
            }
            var children = current.ChildrenGen(gridMap);
            foreach (var child in children)
            {
                xy = gridMap.PositionToGridIndex(child.Pos);
                if (gridMap.Cost(child.Pos) == GridMap.UnReachable)
                    continue;
                if (_closeGrid[xy.x, xy.y])
                    continue;
                openList.Enqueue(child, child.CalcF(to, gridMap));
            }
        }

        if (to.Parent is null)
        {
            _path.Add(new(from.Pos, 0));
            _path.Add(new(to.Pos, 0));
            path = _path;
            return false;
        }
        else
        {
            var k = to;
            _path.Add(new(to.Pos, 0));
            while (k.Parent is not null)
            {
                k = k.Parent;
                _path.Add(new(k.Pos, 0));
            }
            _path.Reverse();
            DownSample();
            path = _path;
            return true;
        }
    }
}