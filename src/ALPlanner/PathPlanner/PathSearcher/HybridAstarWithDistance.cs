using System.Diagnostics;
using ALPlanner.PathPlanner.Nodes;
using Maps;
using TlarcKernel.Extensions.Array;

namespace ALPlanner.PathPlanner.PathSearcher;

using Node = Omni2DConsecNode;

class HybridAStarWithDistance : Component, IPathSearcher
{
  [ComponentReferenceFiled]
  IGridMap gridMap;
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

    Node begin = new(origin);
    Node end = new(target);

    Node.Target = end;
    DateTime dateTime = DateTime.Now;

    _openList.Enqueue(begin, 0);
    while (_openList.Count > 0)
    {
      if ((DateTime.Now - dateTime).Seconds > 0.5)
        return begin;
      var current = _openList.Dequeue();

      if (current.GeometricallyEqualTo(end))
      {
        end.Parent = current;
        break;
      }

      var index = gridMap.PositionInWorldToIndex(current.PositionInWorld);
      if (!gridMap.CheckAccessibility(index))
      {
        continue;
      }

      if (_closeMap.Indexer(index))
        continue;
      _closeMap.Indexer(index, true);

      var children = current.Children;
      foreach (var child in children)
      {
        var childIndex = gridMap.PositionInWorldToIndex(child.PositionInWorld);
        if (
          !gridMap.CheckAccessibility(childIndex)
          || (
            current != begin
            && !gridMap.CheckAccessibility(current.PositionInWorld, child.PositionInWorld)
          )
        )
          continue;
        if (_closeMap.Indexer(childIndex))
          continue;
        _openList.Enqueue(child, child.TotalCost);
      }
    }
    return end;
  }
}
