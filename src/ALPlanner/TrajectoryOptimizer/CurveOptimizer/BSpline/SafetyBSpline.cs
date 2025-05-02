using ALPlanner.TrajectoryOptimizer;
using TlarcKernel.TrajectoryOptimizer.Curves;

namespace Maps;

class SafetyBSpline : Component, IKOrderBSpline
{

    Transform sentry;

    public double[] controlPointsX = [];
    public double[] controlPointsY = [];
    public double[] controlPointsZ = [];
    public double[] timeline = [];


    public double[][] ControlPoint => throw new NotImplementedException();

    public double MaxTime => throw new NotImplementedException();

    public DateTime ConstructTime => throw new NotImplementedException();


    public bool Check() => controlPointsX.Length != 0;

    public void Construction(ConstraintCollection positionList)
    {
        throw new NotImplementedException();
    }

    public void OptimizeTrajectory()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Vector3d> TrajectoryPoints(double fromWhen, double toWhen, double step)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Vector3d> VelocitiesPoints(double fromWhen, double toWhen, double step)
    {
        throw new NotImplementedException();
    }
}