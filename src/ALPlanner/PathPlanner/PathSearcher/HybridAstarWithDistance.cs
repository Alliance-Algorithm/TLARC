
using Maps;
using System.Diagnostics;
using TlarcKernel.Extensions.Array;
using ALPlanner.PathPlanner.Nodes;

namespace ALPlanner.PathPlanner.PathSearcher;

using Node = SpeedDirectNode;

class HybridAStarWithDistance : Component, IPathSearcher
{
    [ComponentReferenceFiled]
    IESDF gridMap;
    PriorityQueue<INode, float> _openList;
    bool[,,] _closeMap;

    public bool Check => throw new NotImplementedException();

    public override void Start()
    {
        Debug.Assert(typeof(Node).GetInterfaces().Contains(typeof(INode)));

        _closeMap = new bool[gridMap.Size.x, gridMap.Size.y, gridMap.Size.z];
    }


    public INode? Search(Vector3d origin, Vector3d target, Vector3d? speed = null)
    {
        Array.Clear(_closeMap, 0, _closeMap.Length);
        _openList = new();

        Node begin = new(origin, speed ?? new Vector3d(0, 0, 0));
        Node end = new(target, new Vector3d(0, 0, 0));

        Node.Target = end;

        _openList.Enqueue(begin, 0);
        while (_openList.Count > 0)
        {
            var current = _openList.Dequeue();

            if (current.GeometricallyEqualTo(end))
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
                if (!gridMap.CheckAccessibility(childIndex, 0) || (current != begin &&
                !gridMap.CheckAccessibility(current.PositionInWorld, child.PositionInWorld)))
                    continue;
                if (_closeMap.Indexer(childIndex))
                    continue;
                _openList.Enqueue(child, child.TotalCost + (float)(100 - gridMap[childIndex]) * 0.1f);
            }
        }
        return end;
    }

}