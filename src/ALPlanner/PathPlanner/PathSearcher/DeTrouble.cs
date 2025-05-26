using System.Diagnostics;
using ALPlanner.PathPlanner.Nodes;
using ALPlanner.PathPlanner.PathSearcher;
using Maps;
using TlarcKernel.Extensions.Array;

namespace ALPlanner.PathPlanner.PathSearcher;

using Node = Omni2DConsecNode;

class DeTrouble : Component, IPathSearcher
{
    [ComponentReferenceFiled]
    IGridMap gridMap;

    PriorityQueue<INode, float> _openList;
    bool[,,] _closeMap;

    public override void Start()
    {
        Debug.Assert(typeof(Node).GetInterfaces().Contains(typeof(INode)));

        _closeMap = new bool[gridMap.Size.x, gridMap.Size.y, gridMap.Size.z];
    }
    public bool Check => throw new NotImplementedException();

    public INode? Search(Vector3d origin, Vector3d target, Vector3d? speed = null)
    {
        Array.Clear(_closeMap, 0, _closeMap.Length);
        _openList = new();

        Node begin = new(origin);
        Node.Target = begin;

        while (_openList.Count > 0)
        {
            var current = _openList.Dequeue();

            if (gridMap.CheckAccessibility(current.PositionInWorld))
                return current;

            var index = gridMap.PositionInWorldToIndex(current.PositionInWorld);

            if (_closeMap.Indexer(index))
                continue;
            _closeMap.Indexer(index, true);

            var children = current.Children;
            foreach (var child in children)
            {
                var childIndex = gridMap.PositionInWorldToIndex(child.PositionInWorld);
                if (_closeMap.Indexer(childIndex))
                    continue;
                if (!gridMap.CheckAccessibility(childIndex))
                {
                    if (child.TotalCost < 2)
                        _openList.Enqueue(child, child.TotalCost);
                }
                else return child;
            }
        }
        return begin;
    }


}