namespace ALPlanner.PathPlanner.PathSearcher;

interface IPathSearcher
{
    public bool Check { get; }
    public Nodes.INode? Search(Vector3d origin, Vector3d target);
}