using TlarcKernel.Transform;
using TlarcKernel;
using ALPlanner.Interfaces;
using ALPlanner.TrajectoryOptimizer;

namespace ALPlanner.PathPlanner;

class ALPlanner : Component
{
    Transform sentry;
    IPositionDecider target;
    PathPlanner pathPlanner;
    Trajectory trajectory;


    private Vector3d lastTarget;
    public override void Update()
    {
        if (target.TargetPosition == lastTarget)
        {
            return;
        }

        trajectory.CalcTrajectory
        (
            pathPlanner.Search(sentry.Position, target.TargetPosition)
        );
    }
}