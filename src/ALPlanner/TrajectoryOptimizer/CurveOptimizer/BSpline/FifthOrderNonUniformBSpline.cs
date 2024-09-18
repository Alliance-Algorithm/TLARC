using g4;

namespace ALPlanner.TrajectoryOptimizer.Curves.BSpline;

class FifthOrderNonUniformBSpline : IKOrderCurve
{
    public void Construction(IEnumerable<Vector3d> positionList, Vector3dTuple2 HeadTailVelocity, Vector3dTuple2 HeadTailAcceleration)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Vector3d> TrajectoryPoints(double fromWhen, double toWhen, double step)
    {
        throw new NotImplementedException();
    }

    public Vector3d Value(double time)
    {
        throw new NotImplementedException();
    }
}