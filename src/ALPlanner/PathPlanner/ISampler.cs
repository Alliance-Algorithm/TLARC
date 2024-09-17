using g4;

namespace ALPlanner.PathPlanner.Sampler;

interface ISampler
{
    IEnumerable<Vector3d> Sample(Nodes.INode? endNode);
}