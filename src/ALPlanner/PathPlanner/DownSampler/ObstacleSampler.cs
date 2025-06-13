using System.ComponentModel;
using ALPlanner.PathPlanner.Nodes;
using Emgu.CV.Reg;
using Maps;

namespace ALPlanner.PathPlanner.Sampler;

class ObstacleSampler : Component, ISampler
{
  private float _samplingRate = 0.2f;

  [ComponentReferenceFiled] IGridMap map;

  public Vector3d[] Sample(INode? endNode)
  {
    Stack<Vector3d> path = new();
    float totalVal = 1;
    var tmp = endNode;
    if (tmp is null)
      return [];
    INode last = tmp;
    while (tmp.Parent is not null)
    {
      totalVal += _samplingRate;
      last = tmp;
      tmp = tmp.Parent;
      if (totalVal < 1) 
        if (map.CheckAccessibility(tmp.PositionInWorld, path.Peek()))
          continue;
      path.Push(last.PositionInWorld);
      totalVal = 0;
    }
    path.Push(tmp.PositionInWorld);
    return [.. path];
  }
}
