
using ALPlanner.TrajectoryOptimizer.Curves;
using Rosidl.Messages.Builtin;
using TlarcKernel;
using TlarcKernel.Transform;

namespace ALPlanner.TrajectoryOptimizer;

class TrajectoryOptimizer : Component
{
    [ComponentReferenceFiled]
    IKOrderCurve kOrderCurve;
    Transform sentry;
    Vector3d lastSentryPosition;
    Vector3d sentryVelocity;

    public DateTime constructTime { get; private set; }
    public double constructTimeToNowInSeconds => (DateTime.Now - constructTime).Duration().TotalSeconds;

    public double MaxTime => kOrderCurve.MaxTime;
    public void CalcTrajectory(Vector3d[] path)
    {
        constructTime = DateTime.Now;
        kOrderCurve.Construction(path, new(sentryVelocity, Vector3d.Zero), new(Vector3d.Zero, Vector3d.Zero));
    }

    public void Construction(Vector3d[] positionList, Vector3dTuple2 HeadTailVelocity, Vector3dTuple2 HeadTailAcceleration)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Vector3d> TrajectoryPoints(double fromWhen, double toWhen, double step)
    => kOrderCurve.TrajectoryPoints(fromWhen, toWhen, step);

    public override void Update()
    {
        sentryVelocity = (sentry.Position - lastSentryPosition) / DeltaTimeInSecond;
        lastSentryPosition = sentry.Position;
    }


}