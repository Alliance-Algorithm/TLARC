
using Vectord = MathNet.Numerics.LinearAlgebra.Vector<double>;
using Matrixd = MathNet.Numerics.LinearAlgebra.Matrix<double>;

namespace ALPlanner.Infrastructure.Optimizer;

public class PolynomialTraj(in int N, in Matrixd c, in Vectord T1, in Vectord T2, in Vectord T3, in Vectord T4, in Vectord T5)
{
    readonly int N = N;
    readonly Matrixd c = c;
    readonly Vectord T1 = T1;
    readonly Vectord T2 = T2;
    readonly Vectord T3 = T3;
    readonly Vectord T4 = T4;
    readonly Vectord T5 = T5;

    internal double AddJCostJerk()
    {
        var cost = 0.0;
        for (int i = 0; i < N; i++)
            cost += 36.0 * c.Row(6 * i + 3).Sum(x => x * x) * T1[i] +
                    144.0 * c.Row(6 * i + 4).DotProduct(c.Row(6 * i + 3)) * T2[i] +
                    192.0 * c.Row(6 * i + 4).Sum(x => x * x) * T3[i] +
                    240.0 * c.Row(6 * i + 5).DotProduct(c.Row(6 * i + 3)) * T3[i] +
                    720.0 * c.Row(6 * i + 5).DotProduct(c.Row(6 * i + 4)) * T4[i] +
                    720.0 * c.Row(6 * i + 5).Sum(x => x * x) * T5[i];
        return cost;
    }

    // \frac{\partial K}{\partial T}
    internal void AddGradJbyT(in Vectord gdT)
    {
        for (int i = 0; i < N; i++)
        {  // E.q(6)(4)
            gdT[i] += 36.0 * c.Row(6 * i + 3).Sum(x => x * x) +
                      288.0 * c.Row(6 * i + 4).DotProduct(c.Row(6 * i + 3)) * T1[i] +
                      576.0 * c.Row(6 * i + 4).Sum(x => x * x) * T2[i] +
                      720.0 * c.Row(6 * i + 5).DotProduct(c.Row(6 * i + 3)) * T2[i] +
                      2880.0 * c.Row(6 * i + 5).DotProduct(c.Row(6 * i + 4)) * T3[i] +
                      3600.0 * c.Row(6 * i + 5).Sum(x => x * x) * T4[i];
        }
    }
    // \frac{\partial K}{\partial c}
    internal void AddGradJbyC(in Matrixd gdC)
    {
        for (int i = 0; i < N; i++)
        {   // Eq.(4)(7)
            gdC.SetRow(6 * i + 5, gdC.Row(6 * i + 5) +
                                    240.0 * c.Row(6 * i + 3) * T3[i] +
                                    720.0 * c.Row(6 * i + 4) * T4[i] +
                                    1440.0 * c.Row(6 * i + 5) * T5[i]);
            gdC.SetRow(6 * i + 4, gdC.Row(6 * i + 4) +
                                    144.0 * c.Row(6 * i + 3) * T2[i] +
                                    384.0 * c.Row(6 * i + 4) * T3[i] +
                                    720.0 * c.Row(6 * i + 5) * T4[i]);
            gdC.SetRow(6 * i + 3, gdC.Row(6 * i + 3) +
                                    72.0 * c.Row(6 * i + 3) * T1[i] +
                                    144.0 * c.Row(6 * i + 4) * T2[i] +
                                    240.0 * c.Row(6 * i + 5) * T3[i]);
        }
        return;
    }
}