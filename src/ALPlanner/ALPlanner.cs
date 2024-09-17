
using g4;
using TlarcKernel.Transform;
using TlarcKernel;
using System.Diagnostics;
using ALPlanner.Interfaces;

namespace ALPlanner.PathPlanner;

class ALPlanner : Component
{
    IPathSearcher pathSearcher;
    ISampler sampler;
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