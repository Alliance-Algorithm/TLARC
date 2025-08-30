
using Vectorf = NumFlat.Vec<double>;
using Matrixf = NumFlat.Mat<double>;
using System.Numerics;
using Kernel.DataInterfaces.Visualization;
using Kernel.DataInterfaces.Constraints;
using CommunityToolkit.HighPerformance;
using NumFlat;

namespace ALPlanner.Infrastructure.Optimizer;


class Diffeomorphism
{
    readonly Vectorf VT;
    readonly Vectorf RT;
    readonly Matrixf Q;
    readonly Matrixf dQ;
    readonly Vectorf gKesi;
    readonly Circle2D[] obstacles;
    internal const double wei_time_ = 1e1;
    static (Vector2 o, Vector2 dir, float r) CircleIntersection(Circle2D a, Circle2D b)
    {
        var sd = (a.Origin - b.Origin).LengthSquared();
        var d = MathF.Sqrt(sd);
        var sa = a.R * a.R;
        var dis = (sa - b.R * b.R + sd) / (2 * d);
        var h = MathF.Sqrt(a.R * a.R - dis * dis);
        var err = (b.Origin - a.Origin) / d;
        var p0 = a.Origin + err * dis;
        (err.X, err.Y) = (err.Y, -err.X);
        return (p0, err, h);
    }
    internal void VTToRT(in Vectorf tau)
    {
        tau.CopyTo(VT);
        for (int i = 0; i < tau.Count; i++)
        {
            RT[i] = tau[i] > 0
                ? tau[i] * tau[i] / 2 + tau[i] + 1
                : 1.0 / (tau[i] * tau[i] / 2 - tau[i] + 1);
        }
    }
    internal void KesiToQ(Vectorf kesi)
    {
        var K = (kesi.Count - (obstacles.Length - 1)) / obstacles.Length / 2 + 1;
        int count = 0;
        for (int i = 0, k = kesi.Count; i < k;)
        {
            var index = count / K;
            if (index == (count + 1) / K)
            {
                var kesi_ = kesi.Subvector(i, 2);
                var tmp = 2 * kesi_ * obstacles[index].R / (kesi_ * kesi_ + 1);
                Q[count, 0] = obstacles[index].Origin.X + tmp[0];
                Q[count, 1] = obstacles[index].Origin.Y + tmp[1];
                i += 2;
                count += 1;
            }
            else
            {
                var kesi_ = kesi[i];
                var (o, dir, r) = CircleIntersection(obstacles[index], obstacles[index + 1]);
                var tmp = 2 * kesi_ * r / (kesi_ * kesi_ + 1);
                Q[count, 0] = o.X + tmp * dir.X;
                Q[count, 1] = o.Y + tmp * dir.Y;
                i += 1;
                count += 1;
            }
        }
    }

    internal Diffeomorphism(in Vectorf RT, in Matrixf Q, in Matrixf dQ, Vectorf gKesi, in Circle2D[] obstacles)
    {
        VT = new Vectorf(RT.Count);
        this.RT = RT;
        this.Q = Q;
        this.obstacles = obstacles;
        this.gKesi = gKesi;
        this.dQ = dQ;
    }

    internal void VirtualTGrad(
            Vectorf gdRT, Vectorf gdVT)
    {
        for (int i = 0; i < VT.Count; ++i)
        {
            double gdVT2Rt;
            if (VT[i] > 0)
            {
                gdVT2Rt = VT[i] + 1.0;
            }
            else
            {
                var denSqrt = (0.5 * VT[i] - 1.0) * VT[i] + 1.0; // 分母的根号部分
                gdVT2Rt = (1.0 - VT[i]) / (denSqrt * denSqrt);
            }
            gdVT[i] = (gdRT[i] + wei_time_) * gdVT2Rt;// add a wei_time*T_sum penalty. 
        }
    }

    internal double AddJCostT() => RT.Sum() * wei_time_;


    /// <summary>
    /// Calculate \frac{\partial J}{\partial kesi}
    /// </summary>
    internal void AddGradQByKesi(Vectorf kesi)
    {
        var K = (kesi.Count - (obstacles.Length - 1)) / obstacles.Length / 2 + 1;
        int count = 0;
        for (int i = 0, k = kesi.Count; i < k;)
        {
            var index = count / K;
            if (index == (count + 1) / K)
            {
                var kesi_ = kesi.Subvector(i, 2);
                var gtg = kesi_ * kesi_ + 1;
                var tmp1 = 2 * dQ.Rows[count] * obstacles[index].R / gtg;
                var tmp2 = 4 * (kesi_ * dQ.Rows[count]) * kesi_ * obstacles[index].R / (gtg * gtg);
                tmp1.SubInplace(tmp2);
                gKesi[i] = tmp1[0];
                gKesi[i + 1] = tmp1[1];
                i += 2;
                count += 1;
            }
            else
            {
                var kesi_ = kesi[i];
                var (_, dir, r) = CircleIntersection(obstacles[index], obstacles[index + 1]);
                var gtg = kesi_ * kesi_ + 1;
                var tmp1 = 2 * dQ.Rows[count] * r / gtg;
                var tmp2 = 4 * (kesi_ * dQ.Rows[count]) * kesi_ * r / (gtg * gtg);
                gKesi[i] = (tmp1 - tmp2)[0] * dir.X + (tmp1 - tmp2)[1] * dir.Y;
                i += 1;
                count += 1;
            }
        }
    }

}