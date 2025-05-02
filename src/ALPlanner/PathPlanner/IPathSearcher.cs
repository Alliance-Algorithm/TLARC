

using Maps;

namespace ALPlanner.PathPlanner.PathSearcher;

interface IPathSearcher
{
  public bool Check { get; }
  public Nodes.INode? Search(Vector3d origin, Vector3d target, Vector3d? speed = null);
}

interface IPathSearcher<T> where T : Maps.IMap
{
  public Nodes.INode? Search(Vector3d origin, Vector3d target, T Map, Vector3d? speed = null);
}
