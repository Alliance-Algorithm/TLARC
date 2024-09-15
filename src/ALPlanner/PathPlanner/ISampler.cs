using g4;

namespace ALPlanner.PathPlanner;

interface ISampler
{
    IEnumerable<Vector3d> Sample(INode? endNode);
}