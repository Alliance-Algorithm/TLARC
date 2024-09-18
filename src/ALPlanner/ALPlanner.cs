
using g4;
using TlarcKernel.Transform;
using TlarcKernel;
using ALPlanner.Interfaces;
using ALPlanner.PathPlanner.PathSearcher;
using ALPlanner.PathPlanner.Sampler;

namespace ALPlanner.PathPlanner;

class ALPlanner : Component
{
    Transform sentry;

    IPositionDecider target;
    private Vector3d lastTarget;
    public override void Update()
    {
        if (target.TargetPosition == lastTarget)
        {
            return;
        }
    }
}