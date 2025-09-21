
using Vectorf = NumFlat.Vec<double>;
using Matrixf = NumFlat.Mat<double>;
using ALPlanner.Infrastructure.Optimizer;
using Kernel.DataInterfaces.Constraints;

namespace ALPlanner.Infrastructure.Optimizer;

public interface IDiffeomorphism
{
    internal void VTToRT(in Vectorf tau);

    internal void KesiToQ(Vectorf kesi);

    internal double AddJCostT();

    internal void VirtualTGrad(Vectorf gdRT, Vectorf gdVT);

    internal void AddGradQByKesi(Vectorf kesi);
}

public static class DiffeomorphismFactory
{
    public static IDiffeomorphism Build<T>(in Vectorf RT, in Matrixf Q, in Matrixf dQ, Vectorf gKesi, in T data) =>
    data switch
    {
        Circle2D[] d => new Diffeomorphism_Circle2D(RT, Q, dQ, gKesi, d),
        _ => throw new NotImplementedException(data.GetType().FullName)
    };
}