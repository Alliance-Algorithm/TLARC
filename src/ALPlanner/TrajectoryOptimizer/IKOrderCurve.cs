using TrajectoryTracer;

namespace ALPlanner.TrajectoryOptimizer;

interface IKOrderCurve : ITrajectory<Vector3d>
{
    public IEnumerable<Vector3d> TrajectoryPoints(double fromWhen, double toWhen, double step);

    public void Construction(Vector3d[] positionList, Vector3dTuple2 HeadTailVelocity, Vector3dTuple2 HeadTailAcceleration);
}