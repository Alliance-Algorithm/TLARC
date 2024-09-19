namespace ALPlanner.PathPlanner.Sampler;

interface ISampler
{
    Vector3d[] Sample(Nodes.INode? endNode);
}