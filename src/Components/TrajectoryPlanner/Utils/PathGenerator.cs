using System.Numerics;

namespace Tlarc.TrajectoryPlanner.Utils;
abstract class PathGenerator : Component
{
    public abstract List<Vector3> Path { get; }
    public abstract bool Search(Vector2 target, out List<Vector3> path);
}