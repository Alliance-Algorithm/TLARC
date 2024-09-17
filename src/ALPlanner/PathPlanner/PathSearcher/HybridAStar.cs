
using Maps;
using g4;
using System.Diagnostics;
using TlarcKernel;
using TlarcKernel.Transform;
using TlarcKernel.Extensions.Array;
using ALPlanner.PathPlanner;
using ALPlanner.PathPlanner.Nodes;
using System.Collections;

namespace ALPlanner.PathPlanner.PathSearching;

using Node = Omni2DConsecNode;

class HybridAStar : Component, IPathSearcher
{
    Transform sentry;
    IGridMap gridMap;

    PriorityQueue<INode, float> _openList;
    bool[,,] _closeMap;

    public override void Start()
    {
        Debug.Assert(typeof(Node).GetInterfaces().Contains(typeof(INode)));

        _closeMap = new bool[gridMap.Size.x, gridMap.Size.y, gridMap.Size.z];
    }

    public INode? Search(Vector3d origin, Vector3d target)
    {
        Array.Clear(_closeMap, 0, _closeMap.Length);
        _openList = new();

        Node begin = new(sentry.Position);
        Node end = new(target);

        Node.Target = end;

        _openList.Enqueue(begin, 0);
        while (_openList.Count > 0)
        {
            var current = _openList.Dequeue();

            if (current.GeometricallyEqualTo(current))
            {
                end.Parent = current.Parent;
                break;
            }

            var index = gridMap.PositionInWorldToIndex(current.PositionInWorld);

            if (_closeMap.Indexer(index))
                continue;
            _closeMap.Indexer(index, true);

            var children = current.Children;
            foreach (var child in children)
            {
                var childIndex = gridMap.PositionInWorldToIndex(child.PositionInWorld);
                if (!gridMap.CheckAccessibility(childIndex))
                    continue;
                if (_closeMap.Indexer(childIndex))
                    continue;
                _openList.Enqueue(child, child.TotalCost);
            }
        }
        Stack<Vector3d> path = new();
        return end;
    }

}