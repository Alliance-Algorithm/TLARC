using System.ComponentModel;
using ALPlanner.PathPlanner.PathSearcher;
using ALPlanner.PathPlanner.Sampler;

namespace ALPlanner.PathPlanner;

class PathPlanner : Component
{
    [ComponentReferenceFiled]
    private IPathSearcher pathSearcher;

    [ComponentReferenceFiled]
    private ISampler sampler;




    public Vector3d[] Search(Vector3d origin, Vector3d target, Vector3d? speed = null)
    => sampler.Sample(pathSearcher.Search(origin, target, speed));
}