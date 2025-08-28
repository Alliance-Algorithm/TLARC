
using Vectord = MathNet.Numerics.LinearAlgebra.Vector<double>;
using Matrixd = MathNet.Numerics.LinearAlgebra.Matrix<double>;
using System.Numerics;
using Kernel.DataInterfaces.Visualization;
using Kernel.DataInterfaces.Constraints;
using CommunityToolkit.HighPerformance;

namespace ALPlanner.Infrastructure.Optimizer;


class Diffeomorphism
{
    readonly Vectord VT;
    readonly Vectord RT;
    readonly Matrixd Q;
    readonly Matrixd dQ;
    readonly Vectord gKesi;
    readonly Circle2D[] obstacles;
    internal const double wei_time_ = 1e4;
    static (Vector2 o, Vector2 dir, double r) CircleIntersection(Circle2D a, Circle2D b)
    {
        var sd = (a.Origin - b.Origin).LengthSquared();
        var d = MathF.Sqrt(sd);
        var sa = a.R * a.R;
        var dis = (sa - b.R * b.R + sd) / (2 * d);
        var h = MathF.Sqrt(a.R * a.R - dis * dis);
        var err = (b.Origin - a.Origin) / d;
        var p0 = a.Origin + err * dis;
        (err.X, err.Y) = (err.Y, -err.X);
        return (p0, err, h * 2);
    }
    internal void VTToRT(Vectord tau)
    {
        tau.CopyTo(VT);
        for (int i = 0; i < tau.Count; i++)
        {
            RT[i] = tau[i] > 0
                ? tau[i] * tau[i] / 2 + tau[i] + 1
                : 1.0 / (tau[i] * tau[i] / 2 - tau[i] + 1);
        }
    }
    internal void KesiToQ(Vectord kesi)
    {
        var K = (kesi.Count - (obstacles.Length - 1)) / obstacles.Length / 2 + 1;
        int count = 0;
        for (int i = 0, k = kesi.Count; i < k;)
        {
            var index = count / K;
            if (index == (count + 1) / K)
            {
                var kesi_ = kesi.SubVector(i, 2);
                var tmp = 2 * kesi_ * obstacles[index].R / (kesi_.DotProduct(kesi_) + 1);
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

    internal Diffeomorphism(in Vectord RT, in Matrixd Q, in Matrixd dQ, Vectord gKesi, in Circle2D[] obstacles)
    {
        VT = Vectord.Build.Dense(RT.Count);
        this.RT = RT;
        this.Q = Q;
        this.obstacles = obstacles;
        this.gKesi = gKesi;
        this.dQ = dQ;
    }

    internal void VirtualTGrad(
            Vectord gdRT, Vectord gdVT)
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
                double denSqrt = (0.5 * VT[i] - 1.0) * VT[i] + 1.0; // 分母的根号部分
                gdVT2Rt = (1.0 - VT[i]) / (denSqrt * denSqrt);
            }
            gdVT[i] += (gdRT[i] + wei_time_) * gdVT2Rt;// add a wei_time*T_sum penalty. 
        }
    }

    internal double AddJCostT() => RT.Sum() * wei_time_;


    /// <summary>
    /// Calculate \frac{\partial J}{\partial kesi}
    /// </summary>
    internal void AddGradQByKesi(Vectord kesi)
    {
        var K = (kesi.Count - (obstacles.Length - 1)) / obstacles.Length / 2 + 1;
        int count = 0;
        for (int i = 0, k = kesi.Count; i < k;)
        {
            var index = count / K;
            if (index == (count + 1) / K)
            {
                var kesi_ = kesi.SubVector(i, 2);
                var gtg = kesi_.DotProduct(kesi_) + 1;
                var tmp1 = 2 * dQ.Row(count) * obstacles[index].R / gtg;
                var tmp2 = 4 * (kesi_ * dQ.Row(count)) * kesi_ * obstacles[index].R / (gtg * gtg);
                gKesi.SetSubVector(i, 2, tmp1 - tmp2);
                i += 2;
                count += 1;
            }
            else
            {
                var kesi_ = kesi[i];
                var (o, dir, r) = CircleIntersection(obstacles[index], obstacles[index + 1]);
                var gtg = kesi_ * kesi_ + 1;
                var tmp1 = 2 * dQ.Row(count) * obstacles[index].R / gtg;
                var tmp2 = 4 * (kesi_ * dQ.Row(count)) * kesi_ * obstacles[index].R / (gtg * gtg);
                gKesi[i] = (tmp1 - tmp2)[0] * dir.X + (tmp1 - tmp2)[1] * dir.Y;
                i += 1;
                count += 1;
            }
        }
    }

}