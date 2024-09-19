
namespace ALPlanner.TrajectoryOptimizer.Curves.BSpline;

class FifthOrderNonUniformBSpline : IKOrderCurve
{
    public void Construction(Vector3d[] positionList, Vector3dTuple2 HeadTailVelocity, Vector3dTuple2 HeadTailAcceleration)
    {
        throw new System.NotImplementedException();
    }

    public IEnumerable<Vector3d> TrajectoryPoints(double fromWhen, double toWhen, double step)
    {
        throw new System.NotImplementedException();
    }

    public Vector3d Value(double time)
    {
        throw new System.NotImplementedException();
    }
}