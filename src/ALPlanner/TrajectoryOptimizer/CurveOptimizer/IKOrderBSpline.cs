using ALPlanner.TrajectoryOptimizer.Curves;

namespace TlarcKernel.TrajectoryOptimizer.Curves;

interface IKOrderBSpline : IKOrderCurve
{
    public double[][] ControlPoint { get; }
}