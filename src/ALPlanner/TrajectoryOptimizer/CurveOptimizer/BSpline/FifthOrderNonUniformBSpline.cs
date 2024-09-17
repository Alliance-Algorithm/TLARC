using g4;

namespace ALPlanner.TrajectoryOptimizer.Curves;

class FifthOrderNonUniformBSpline : IKOrderCurve
{
    public void Construction(IEnumerable<Vector3d> positionList, Vector3dTuple2 HeadTailVelocity, Vector3dTuple2 HeadTailAcceleration)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Vector3d> TrajectoryPoints(float fromWhen, float toWhen, float step)
    {
        throw new NotImplementedException();
    }

    public Vector3d Value(float time)
    {
        throw new NotImplementedException();
    }
}