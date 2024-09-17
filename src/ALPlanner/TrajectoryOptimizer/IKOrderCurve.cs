using g4;

namespace ALPlanner.TrajectoryOptimizer.Curves;

interface IKOrderCurve
{
    public IEnumerable<Vector3d> TrajectoryPoints(float fromWhen, float toWhen, float step);

    public Vector3d Value(float time);

    public void Construction(IEnumerable<Vector3d> positionList, Vector3dTuple2 HeadTailVelocity, Vector3dTuple2 HeadTailAcceleration);
}