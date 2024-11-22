using System.ComponentModel;
using ALPlanner.Maps;
using TlarcKernel.TrajectoryOptimizer.Curves;
using TlarcKernel.TrajectoryOptimizer.Optimizer;

namespace ALPlanner.TrajectoryOptimizer.Optimizer;


class ControlPointOptimizer : Component, IOptimizer
{
    private double gradientRatio = 0.1f;
    SemanticESDF semanticESDF;


    public void Optimize(IKOrderBSpline kOrderBSpline)
    {
        var controlPoints = kOrderBSpline.ControlPoint;
        for (int i = 1; i < controlPoints.Length - 1; i++)
        {
            var g = semanticESDF.Gradient(new(controlPoints[i][0], controlPoints[i][1], controlPoints[i][2]));
            controlPoints[i][0] += g.x * gradientRatio;
            controlPoints[i][1] += g.y * gradientRatio;
            controlPoints[i][2] += g.z * gradientRatio;
        }
    }
}