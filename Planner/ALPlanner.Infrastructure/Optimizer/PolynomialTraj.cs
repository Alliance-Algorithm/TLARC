
using Vectorf = NumFlat.Vec<double>;
using Matrixf = NumFlat.Mat<double>;
using System.Numerics;
using NumFlat;

namespace ALPlanner.Infrastructure.Optimizer;

public class PolynomialTraj(in int N, in Matrixf c, in Vectorf T1, in Vectorf T2, in Vectorf T3, in Vectorf T4, in Vectorf T5, in Vectorf gdT, in Matrixf gdC)
{
    readonly int N = N;
    readonly Matrixf c = c;
    readonly Vectorf T1 = T1;
    readonly Vectorf T2 = T2;
    readonly Vectorf T3 = T3;
    readonly Vectorf T4 = T4;
    readonly Vectorf T5 = T5;
    internal readonly Matrixf gdC = gdC;
    internal readonly Vectorf gdT = gdT;

    const double max_vel_ = 1.9;
    // Magic! $max_acc_ < $max_vel_ * 2
    const double max_acc_ = 4.9;
    // Magic! $wei_time_ *  $max_vel_  in Diffeomorphism is wei_feas_mod_
    const double wei_feas_mod_ = 1e4;

    internal double Costs { get; private set; }

    const int K = 10;

    internal void Calculate()
    {
        Costs = 0;
        for (int i = 0; i < N; ++i)
        {
            Matrixf c = this.c.Submatrix(i * 6, 0, 6, 2);
            var step = T1[i] / K;
            var s1 = 0.0;
            // The velocity, position, or the acc in a piece (may has many constraint points) are all determined   // by the c^i and T^i.We should first find the cost function gradients w.r.t p,v,a ( variable just in 
            // the function formulation). Then p,v,a gradient w.r.t the c^i and T^i can be determined by:
            // p(i_j)=c^i * \beta(T^i/K * j)
            // v(i_j)=c^i * \beta^1(T^i/K * j)
            // The gradient relation is sparse, facilating the coding and optimization.
            for (int j = 0; j <= K; ++j)
            {
                var s2 = s1 * s1;
                var s3 = s2 * s1;
                var s4 = s2 * s2;
                Vectorf beta1 = [0.0, 1.0, 2.0 * s1, 3.0 * s2, 4.0 * s3, 5.0 * s4];
                Vectorf beta2 = [0.0, 0.0, 2.0, 6.0 * s1, 12.0 * s2, 20.0 * s3];
                Vectorf beta3 = [0.0, 0.0, 0.0, 6.0, 24.0 * s1, 60.0 * s2];
                var alpha = 1.0 / K * j;
                var ct = c.Transpose();
                var vel = ct * beta1;
                var acc = ct * beta2;
                var jer = ct * beta3;

                s1 += step;
                // The first ponit of the next piece and the first point of current piece will be the same
                // So 0.5 make each point has the same 1 weight  
                var omg = (j == 0 || j == K) ? 0.5 : 1.0;



                // feasibility
                // the feasibility cost = \Sum Ti/K * penalty()
                // so the gradient w.r.t the T has two term according to the chain rule
                // It's a common constraint handle method in MINCO 
                if (FeasibilityGradCostV(vel, out var gradv, out var costv))
                {

                    var gradViolaVc = beta1.ToColMatrix() * gradv;
                    var gradViolaVt = alpha * gradv.Rows[0] * acc;    // Eq.(17)
                    gdC.Submatrix(i * 6, 0, 6, 2).AddInplace(omg * step * gradViolaVc); //Eq.(16)
                    gdT[i] += omg * (costv / K + step * gradViolaVt); // Eq.(18)
                    Costs += omg * step * costv;
                }

                if (FeasibilityGradCostA(acc, out var grada, out var costa))
                {
                    var gradViolaAc = beta2.ToColMatrix() * grada;
                    var gradViolaAt = alpha * grada.Rows[0] * jer;    // Eq.(17)
                    gdC.Submatrix(i * 6, 0, 6, 2).AddInplace(omg * step * gradViolaAc); //Eq.(16)
                    gdT[i] += omg * (costa / K + step * gradViolaAt); // Eq.(18)
                    Costs += omg * step * costa;
                }
            }
        }
    }
    internal double AddJCostJerk()
    {
        var cost = 0.0;
        for (int i = 0; i < N; i++)
            cost += 36.0 * c.Rows[6 * i + 3].Sum(x => x * x) * T1[i] +
                    144.0 * c.Rows[6 * i + 4].Dot(c.Rows[6 * i + 3]) * T2[i] +
                    192.0 * c.Rows[6 * i + 4].Sum(x => x * x) * T3[i] +
                    240.0 * c.Rows[6 * i + 5].Dot(c.Rows[6 * i + 3]) * T3[i] +
                    720.0 * c.Rows[6 * i + 5].Dot(c.Rows[6 * i + 4]) * T4[i] +
                    720.0 * c.Rows[6 * i + 5].Sum(x => x * x) * T5[i];
        return cost;
    }

    // \frac{\partial K}{\partial T}
    internal void AddGradJbyT()
    {
        for (int i = 0; i < N; i++)
        {  // E.q(6)(4)
            gdT[i] += 36.0 * c.Rows[6 * i + 3].Sum(x => x * x) +
                      288.0 * c.Rows[6 * i + 4].Dot(c.Rows[6 * i + 3]) * T1[i] +
                      576.0 * c.Rows[6 * i + 4].Sum(x => x * x) * T2[i] +
                      720.0 * c.Rows[6 * i + 5].Dot(c.Rows[6 * i + 3]) * T2[i] +
                      2880.0 * c.Rows[6 * i + 5].Dot(c.Rows[6 * i + 4]) * T3[i] +
                      3600.0 * c.Rows[6 * i + 5].Sum(x => x * x) * T4[i];
        }
    }
    // \frac{\partial K}{\partial c}
    internal void AddGradJbyC()
    {
        for (int i = 0; i < N; i++)
        {   // Eq.(4)(7)
            gdC.Rows[6 * i + 5].AddInplace(
                                    240.0 * T3[i] * c.Rows[6 * i + 3] +
                                    720.0 * T4[i] * c.Rows[6 * i + 4] +
                                    1440.0 * T5[i] * c.Rows[6 * i + 5]);
            gdC.Rows[6 * i + 4].AddInplace(
                                    144.0 * T2[i] * c.Rows[6 * i + 3] +
                                    384.0 * T3[i] * c.Rows[6 * i + 4] +
                                    720.0 * T4[i] * c.Rows[6 * i + 5]);
            gdC.Rows[6 * i + 3].AddInplace(
                                    72.0 * T1[i] * c.Rows[6 * i + 3] +
                                    144.0 * T2[i] * c.Rows[6 * i + 4] +
                                    240.0 * T3[i] * c.Rows[6 * i + 5]);
        }
        return;
    }

    static bool FeasibilityGradCostV(Vectorf v,
                             out Matrixf gradv,
                             out double costv)
    {
        costv = 0;
        var v_squnorm = v.Sum(x => x * x);
        var vpen = v_squnorm - max_vel_ * max_vel_;
        if (vpen > 0)
        {
            gradv = [wei_feas_mod_ * 6 * vpen * vpen * v];
            costv = wei_feas_mod_ * vpen * vpen * vpen;

            return true;
        }
        else
            gradv = new Matrixf(1, 2);
        return false;
    }
    static bool FeasibilityGradCostA(Vectorf a,
                              out Matrixf grada,
                              out double costa)
    {
        costa = 0;
        var a_squnorm = a.Sum(x => x * x);
        var apen = a_squnorm - max_acc_ * max_acc_;
        if (apen > 0)
        {
            grada = [wei_feas_mod_ * 6 * apen * apen * a];
            costa = wei_feas_mod_ * apen * apen * apen;

            return true;
        }
        else
            grada = new Matrixf(1, 2);
        return false;
    }
}