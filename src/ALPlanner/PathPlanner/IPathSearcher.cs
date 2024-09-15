using g4;

namespace ALPlanner.PathPlanner;

interface IPathSearcher
{
    public INode? Search(Vector3d target);

}