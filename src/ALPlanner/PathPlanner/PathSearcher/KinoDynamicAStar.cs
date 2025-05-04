using System.Diagnostics;
using ALPlanner.PathPlanner.Nodes;
using Maps;
using TlarcKernel.Extensions.Array;

namespace ALPlanner.PathPlanner.PathSearcher;

using Node = AccelerateNode;

class KinoDynamicAStar : Component, IPathSearcher<SafeCorridor>
{

    PriorityQueue<INode, float> _openList = new();
    Transform sentry;
    int[,,] _closeMap;

    public bool Check => throw new NotImplementedException();

    public override void Start()
    {
        Debug.Assert(typeof(Node).GetInterfaces().Contains(typeof(INode)));

    }
    public INode? Search(Vector3d origin, Vector3d target, SafeCorridor Map, Vector3d? speed = null)
    {

        _openList = new();
        _closeMap = new int[Map.Count, 17, 17];
        Node begin = new(origin, sentry.Velocity);
        Node end = new(target, Vector3d.Zero);

        _openList.Enqueue(begin, 0);

        while (_openList.Count > 0)
        {
            var current = _openList.Dequeue();

            foreach (var c in current.Children)
            {

                var index = Map.FindIndex(current.PositionInWorld, c.PositionInWorld);
                var vIndexX = (int)Math.Round(Math.Abs((c as Node).SpeedInWorld.x) / 0.5);
                var vIndexY = (int)Math.Round(Math.Abs((c as Node).SpeedInWorld.y) / 0.5);
                if (index == -1)
                    continue;

                if ((c as Node).SpeedInWorld.Length > 7)
                    continue;
                if (_closeMap[index, vIndexX, vIndexY] >= index)
                    continue;

                _closeMap[index, vIndexX, vIndexY]++;
                _openList.Enqueue(c,
                (float)(c.TotalCost - index * 0.1 + (c.PositionInWorld - end.PositionInWorld).Length
                - (end.PositionInWorld - c.PositionInWorld).Normalized.Dot((c as Node).SpeedInWorld)
                - 0.1 * Map.Field(index).Dot((c as Node).SpeedInWorld)));
                if (c.GeometricallyEqualTo(end))
                {
                    end.Parent = current;
                    return end;
                }
            }
        }
        return null;
    }
}
