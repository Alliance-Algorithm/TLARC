using ALPlanner.Interfaces;
using ALPlanner.Collider;
using TlarcKernel;
using Maps;

namespace ALPlanner;

class ALPlanner : Component
{
    [ComponentReferenceFiled]
    ICollider sentry;

    [ComponentReferenceFiled]
    IPositionDecider target;

    [ComponentReferenceFiled]
    IGridMap gridMap;
    PathPlanner.PathPlanner pathPlanner;
    TrajectoryOptimizer.TrajectoryOptimizer trajectoryOptimizer;

    IO.ROS2Msgs.Nav.Path debugPath1;
    IO.ROS2Msgs.Nav.Path debugPath2;
    Vector3d[] path = [];
    Vector3d[] trajectory = [];
    private Vector3d lastTarget;
    public override void Start()
    {
#if TLARC_DEBUG
        debugPath1 = new(IOManager);
        debugPath1.RegistryPublisher("debug/path");
        debugPath2 = new(IOManager);
        debugPath2.RegistryPublisher("debug/trajectory");
#endif
    }
    public override void Update()
    {
        bool check = true;
        foreach (var point in path)
        {
            if (!gridMap.CheckAccessibility(
             gridMap.PositionInWorldToIndex(point), 0))
            {
                check = false;
                break;
            }
        }
        foreach (var point in trajectory)
        {
            if (check == false || !gridMap.CheckAccessibility(
             gridMap.PositionInWorldToIndex(point), 0))
            {
                check = false;
                break;
            }
        }
        var t = trajectoryOptimizer.constructTimeToNowInSeconds;
        var traj = trajectoryOptimizer.TrajectoryPoints(t - 0.1f, t, 0.1f);
        check &= (sentry.Position - traj.First()).LengthSquared < 1 || gridMap.CheckAccessibility(sentry.Position, traj.First(), 0);

        if (check && target.TargetPosition == lastTarget)
            return;

        path = pathPlanner.Search(sentry.Position, target.TargetPosition);
        if (path.Length < 2)
            return;
        trajectoryOptimizer.CalcTrajectory(path);

        trajectory = trajectoryOptimizer.TrajectoryPoints(0, trajectoryOptimizer.MaxTime, trajectoryOptimizer.MaxTime / 50).ToArray();
        lastTarget = target.TargetPosition;

#if TLARC_DEBUG
        debugPath1.Publish(path);
        debugPath2.Publish(trajectory);
#endif
    }
}