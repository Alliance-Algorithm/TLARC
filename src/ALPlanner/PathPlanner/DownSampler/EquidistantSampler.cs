using System.ComponentModel;
using ALPlanner.PathPlanner.Nodes;

namespace ALPlanner.PathPlanner.Sampler;

class EquidistantSampler : Component, ISampler
{
  private float _samplingRate = 0.1f;

  public Vector3d[] Sample(INode? endNode)
  {
    Stack<Vector3d> path = new();
    float totalVal = 1;
    var tmp = endNode;
    if (tmp is null)
      return [];
    while (tmp.Parent is not null)
    {
      totalVal += _samplingRate;
      tmp = tmp.Parent;
      if (totalVal < 1)
        continue;
      path.Push(tmp.PositionInWorld);
      totalVal = 0;
    }
    path.Push(tmp.PositionInWorld);
    return [.. path];
  }
}
