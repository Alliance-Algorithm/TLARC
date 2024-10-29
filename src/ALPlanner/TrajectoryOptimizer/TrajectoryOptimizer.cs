
using ALPlanner.TrajectoryOptimizer.Curves;
using Rosidl.Messages.Builtin;
using TlarcKernel;
using TlarcKernel.Transform;

namespace ALPlanner.TrajectoryOptimizer;

class TrajectoryOptimizer : Component
{
    IKOrderCurve kOrderCurve;
    Transform sentry;
    Vector3d lastSentryPosition;
    Vector3d sentryVelocity;

    DateTime constructTime;
    double constructTimeToNowInSeconds;

    public void CalcTrajectory(Vector3d[] path)
    {
        constructTime = DateTime.Now;
        kOrderCurve.Construction(path, new(sentryVelocity, Vector3d.Zero), new(Vector3d.Zero, Vector3d.Zero));
    }
    public override void Update()
    {
        constructTimeToNowInSeconds = (constructTime - constructTime).Duration().TotalSeconds;
        sentryVelocity = (sentry.Position - lastSentryPosition) * Program.GetProcessWithPID(ProcessID).Fps;
        lastSentryPosition = sentry.Position;
    }


}