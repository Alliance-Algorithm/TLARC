using TlarcKernel.Transform;
using TlarcKernel;
using ALPlanner.Interfaces;

namespace ALPlanner;

class ALPlanner : Component
{
    Transform sentry;
    IPositionDecider target;
    PathPlanner.PathPlanner pathPlanner;
    TrajectoryOptimizer.TrajectoryOptimizer trajectoryOptimizer;


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