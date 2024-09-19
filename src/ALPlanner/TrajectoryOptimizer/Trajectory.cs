
using ALPlanner.TrajectoryOptimizer.Curves;
using TlarcKernel;
using TlarcKernel.Transform;

namespace ALPlanner.TrajectoryOptimizer;

class Trajectory : Component
{
    IKOrderCurve kOrderCurve;
    Transform sentry;
    Vector3d lastSentryPosition;
    Vector3d sentryVelocity;

    public void CalcTrajectory(Vector3d[] path)
    {
        kOrderCurve.Construction(path, new(sentryVelocity, Vector3d.Zero), new(Vector3d.Zero, Vector3d.Zero));
    }
    public override void Update()
    {
        sentryVelocity = (sentry.Position - lastSentryPosition) * Program.GetProcessWithPID(ProcessID).fps;
        lastSentryPosition = sentry.Position;
    }


}