using TlarcKernel.TrajectoryOptimizer.Curves;

namespace TlarcKernel.TrajectoryOptimizer.Optimizer;

interface IOptimizer
{
  public void Optimize(IKOrderBSpline kOrderBSpline);
}
