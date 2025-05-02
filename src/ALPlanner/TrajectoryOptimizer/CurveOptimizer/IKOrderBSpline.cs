using ALPlanner.TrajectoryOptimizer;

namespace TlarcKernel.TrajectoryOptimizer.Curves;

interface IKOrderBSpline : IKOrderCurve
{
  public double[][] ControlPoint { get; }
}
