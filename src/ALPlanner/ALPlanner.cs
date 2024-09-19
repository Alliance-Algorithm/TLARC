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
    TrajectoryOptimizer trajectoryOptimizer;


    private Vector3d lastTarget;
    public override void Update()
    {
        if (target.TargetPosition == lastTarget)
        {
            return;
        }
        do
        {
            trajectoryOptimizer.CalcTrajectory
            (
                pathPlanner.Search(sentry.Position, target.TargetPosition)
            );
        }
        while (true);
    }
}