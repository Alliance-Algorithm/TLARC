using TrajectoryTracer;

namespace ALPlanner.TrajectoryOptimizer;

interface IKOrderCurve
{

    public double MaxTime { get; }
    public DateTime ConstructTime { get; }
    public IEnumerable<Vector3d> TrajectoryPoints(double fromWhen, double toWhen, double step);

    public void Construction(Vector3d[] positionList, Vector3dTuple2 HeadTailVelocity, Vector3dTuple2 HeadTailAcceleration);
}