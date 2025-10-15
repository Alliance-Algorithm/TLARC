
using Vectorf = NumFlat.Vec<double>;
using Matrixf = NumFlat.Mat<double>;
using System.Numerics;
using Kernel.Contract.Visualization;
using Kernel.Contract.Constraints;
using CommunityToolkit.HighPerformance;
using NumFlat;

namespace ALPlanner.Infrastructure.Optimizer;



unsafe internal readonly struct Diffeomorphism_AABB : IDiffeomorphism
{
    readonly Vectorf VT;
    readonly Vectorf RT;
    readonly Matrixf Q;
    readonly Matrixf dQ;
    readonly Vectorf gKesi;
    readonly AABB2D[] obstacles;
    readonly record struct VHat(in Vectorf V0, in Matrixf Mat);
    readonly VHat[] vHat;
    private readonly int N;

    readonly Vectorf Gd;
    readonly int Length;

    readonly Vectorf IDiffeomorphism.Gd => Gd;

    readonly int IDiffeomorphism.Length => Length;

    internal const double wei_time_ = 1e4;

    public void VTToRT(in Vectorf tau)
    {
        tau.CopyTo(VT);
        for (int i = 0; i < tau.Count; i++)
        {
            RT[i] = tau[i] > 0
                ? tau[i] * tau[i] / 2 + tau[i] + 1
                : 1.0 / (tau[i] * tau[i] / 2 - tau[i] + 1);
        }
    }

    static AABB2D CombineAABB(AABB2D a, AABB2D b) =>
          new()
          {
             MinX = Math.Max(a.MinX, b.MinX), MinY = Math.Max(b.MinY, a.MinY),
             MaxX = Math.Min(a.MaxX, b.MaxX), MaxY = Math.Min(a.MaxY, b.MaxY)};
    public void KesiToQ(Vectorf kesi)
    {
        var K = N / obstacles.Length;
        int count = 0;
        for (int i = 0, k = kesi.Count; i < k;)
        {
            var index = count / K;
            var kesi_ = kesi.Subvector(i, 3);
            var t1 = kesi_ * kesi_ + 1;
            var tmp = vHat[index].V0 + 4 * vHat[index].Mat * kesi_.PointwiseMul(kesi_) / (t1 * t1);
            Q[count, 0] = tmp[0];
            Q[count, 1] = tmp[1];
            i += 3;
            count += 1;
        }
    }

    public Diffeomorphism_AABB(in Vectorf RT, in Matrixf Q, in Matrixf dQ, in int N, in AABB2D[] obstacles)
    {
        Length = (N - 1) * 3 + N;
        this.N = N;
        this.Gd = new Vectorf(Length);
        this.VT = new Vectorf(RT.Count);
        this.RT = RT;
        this.Q = Q;
        this.obstacles = obstacles;
        this.gKesi = Gd[N..];
        this.dQ = dQ;
        this.vHat = new VHat[N - 1];
        var K = N / obstacles.Length;
        int count = 0;
        while ((count + 1) / K < obstacles.Length)
        {
            var index = count / K;
            switch (index != (count + 1) / K)
            {

                case false:
                    var x1 = obstacles[index].MaxX - obstacles[index].MinX;
                    var y1 = obstacles[index].MaxY - obstacles[index].MinY;
                    vHat[index] = new([obstacles[index].MinX, obstacles[index].MinY],
                    new(2, 3, 2, new double[] {
                                            0, y1,
                                            x1, y1,
                                            x1, 0
                                            }));
                    break;


                case true:
                    var combined = CombineAABB(obstacles[index + 1], obstacles[index]);
                    var x2 = combined.MaxX - combined.MinX;
                    var y2 = combined.MaxY - combined.MinY;
                    vHat[index] = new([combined.MinX, combined.MinY],
                    new(2, 3, 2, new double[] {
                                            0, y2,
                                            x2, y2,
                                            x2, 0
                                            }));
                    break;
            }
            ; count++;
        }
    }

    public void VirtualTGrad(
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

    public double AddJCostT() => RT.Sum() * wei_time_;


    /// <summary>
    /// Calculate \frac{\partial J}{\partial kesi}
    /// </summary>
    public void AddGradQByKesi(Vectorf kesi)
    {
        var K = (kesi.Count - (obstacles.Length - 1)) / obstacles.Length / 2 + 1;
        int count = 0;

        for (int i = 0, k = kesi.Count; i < k;)
        {
            var index = count / K;
            var kesi_ = kesi.Subvector(i, 3);
            var gtg = kesi_ * kesi_ + 1;
            var gtg2 = gtg * gtg;
            var tmp1 = 8 * kesi_.PointwiseMul(vHat[index].Mat.Transpose() * dQ.Rows[count]) / gtg2;
            var tmp2 = 16 * (dQ.Rows[count].ToRowMatrix() * vHat[index].Mat * kesi_.PointwiseMul(kesi_))[0] / (gtg2 * gtg) * kesi_;
            tmp1.SubInplace(tmp2);
            gKesi[i] = tmp1[0];
            gKesi[i + 1] = tmp1[1];
            gKesi[i + 2] = tmp1[2];
            i += 3;
            count += 1;
        }
    }

    void IDiffeomorphism.VTToRT(in Vectorf tau)
    {
        VTToRT(tau);
    }

    void IDiffeomorphism.KesiToQ(Vectorf kesi)
    {
        KesiToQ(kesi);
    }

    double IDiffeomorphism.AddJCostT()
    {
        return AddJCostT();
    }

    void IDiffeomorphism.VirtualTGrad(Vectorf gdRT, Vectorf gdVT)
    {
        VirtualTGrad(gdRT, gdVT);
    }

    void IDiffeomorphism.AddGradQByKesi(Vectorf kesi)
    {
        AddGradQByKesi(kesi);
    }
}