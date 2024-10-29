using System.ComponentModel;
using ALPlanner.PathPlanner.PathSearcher;
using ALPlanner.PathPlanner.Sampler;

namespace ALPlanner.PathPlanner;

class PathPlanner : Component
{
    private IPathSearcher pathSearcher;
    private ISampler sampler;



    public Vector3d[] Search(Vector3d origin, Vector3d target)
    => sampler.Sample(pathSearcher.Search(origin, target));
}