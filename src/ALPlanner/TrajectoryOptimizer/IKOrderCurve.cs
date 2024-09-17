using g4;

namespace ALPlanner.TrajectoryOptimizer.Curves;

interface IKOrderCurve
{
    public IEnumerable<Vector3d> TrajectoryPoints(double fromWhen, double toWhen, double step);

    public Vector3d Value(double time);

    public void Construction(IEnumerable<Vector3d> positionList, Vector3dTuple2 HeadTailVelocity, Vector3dTuple2 HeadTailAcceleration);
}