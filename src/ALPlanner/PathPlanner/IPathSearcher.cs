using g4;

namespace ALPlanner.PathPlanner.PathSearcher;

interface IPathSearcher
{
    public Nodes.INode? Search(Vector3d origin, Vector3d target);

}