using TrajectoryTracer;

namespace ALPlanner.TrajectoryOptimizer.Curves;

interface IKOrderCurve : ITrajectory
{
    public IEnumerable<Vector3d> TrajectoryPoints(double fromWhen, double toWhen, double step);

    public void Construction(Vector3d[] positionList, Vector3dTuple2 HeadTailVelocity, Vector3dTuple2 HeadTailAcceleration);
}