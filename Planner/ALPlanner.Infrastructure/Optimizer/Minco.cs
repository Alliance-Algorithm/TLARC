/*
使用N = 7 s = 3(jerk)的多项式
*/
using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using CommunityToolkit.HighPerformance;
using Kernel.DataInterfaces.Navigation;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Vectord = MathNet.Numerics.LinearAlgebra.Vector<double>;
using Matrixd = MathNet.Numerics.LinearAlgebra.Matrix<double>;
using Kernel.DataInterfaces.Constraints;

namespace ALPlanner.Infrastructure.Optimizer;

public class Minco
{
    internal readonly int N;
    internal const int S = 3;
    internal readonly Matrixd A;
    internal readonly Vectord T1;
    internal readonly Vectord T2;
    internal readonly Vectord T3;
    internal readonly Vectord T4;
    internal readonly Vectord T5;
    internal readonly Matrixd c;
    internal readonly Matrixd gC;
    internal readonly Vectord gT;
    internal readonly Matrixd gQ;
    internal readonly Vectord gKesi;
    readonly PolynomialTraj poly;
    readonly MincoTraj minco;
    readonly Diffeomorphism diffeomorphism;

    readonly public Vector2[] _headPVA;
    readonly public Vector2[] _tailPVA;

    readonly Matrixd Q;

    readonly Circle2D[] _obstacles;

    public double TotalSecond { get; private set; }

    public Minco(in int N, in Circle2D[] obstacles)
    {
        this.N = N;
        this._headPVA = new Vector2[3];
        this._tailPVA = new Vector2[3];
        this.A = Matrixd.Build.Dense(2 * S * N, 2 * S * N);
        this.T1 = Vectord.Build.Dense(N);
        this.T2 = Vectord.Build.Dense(N);
        this.T3 = Vectord.Build.Dense(N);
        this.T4 = Vectord.Build.Dense(N);
        this.T5 = Vectord.Build.Dense(N);
        this.c = Matrixd.Build.Dense(2 * S * N, 2);
        this.gC = Matrixd.Build.Dense(2 * S * N, 2);
        this.gT = Vectord.Build.Dense(N);
        this.gQ = Matrixd.Build.Dense(N - 1, 2);
        this.gKesi = Vectord.Build.Dense((N - 1) * 2 - obstacles.Length + 1);
        this.Q = Matrixd.Build.Dense(N - 1, 2);
        this._obstacles = obstacles;
        poly = new PolynomialTraj(N, c, T1, T2, T3, T4, T5);
        minco = new MincoTraj(c, N, T1, T2, T3, T4, T5, A, _headPVA, _tailPVA);
        diffeomorphism = new Diffeomorphism(T1, Q, gQ, gKesi, _obstacles);
    }



    public void Generate(in Vectord tau, Vectord kesi)
    {
        diffeomorphism.VTToRT(tau);
        diffeomorphism.KesiToQ(kesi);
        minco.Generate(Q);
        TotalSecond = T1.Sum();
    }

    public double GetJ() =>
        diffeomorphism.AddJCostT() +
        poly.AddJCostJerk();


    /// <summary>
    /// <para>@param:[in,out] gT. In:\frac{\partial K}{\partial T} Out:\frac{\partial W}{\partial T}.</para>
    /// @param:[out] gQ. \frac{\partial W}{\partial q}
    /// </summary>
    /// <param name="gT"></param>
    /// <param name="gQ"></param>
    public Vectord GetGradient(Vectord kesi, Vectord paras)
    {
        gT.Clear();
        gQ.Clear();
        gC.Clear();

        poly.AddGradJbyC(gC);
        poly.AddGradJbyT(gT);

        A.Transpose().LU().Solve(gC, gC);

        // Given G, \frac{\partial K}{\partial T} get the \frac{\partial W}{\partial T}.
        minco.AddPropCtoT(gC, gT);
        // Given G, get the \frac{\partial W}{\partial q}
        minco.AddPropCtoP(gC, gQ);

        diffeomorphism.VirtualTGrad(gT, gT);
        diffeomorphism.AddGradQByKesi(kesi);
        paras.SetSubVector(0, N, gT);
        paras.SetSubVector(N, gKesi.Count, gKesi);
        return paras;
    }

    public Vector2 GetPosition(double t)
    {
        double t2, t3, t4, t5;
        int i;
        for (i = 0; i < N; i++)
        {
            if (t < T1[i])
                break;
            t -= T1[i];
        }
        t2 = t * t;
        t3 = t2 * t;
        t4 = t3 * t;
        t5 = t4 * t;
        return new(
        (float)((c[(6 * i) + 0, 0] * 1) +
                (c[(6 * i) + 1, 0] * t) +
                (c[(6 * i) + 2, 0] * t2) +
                (c[(6 * i) + 3, 0] * t3) +
                (c[(6 * i) + 4, 0] * t4) +
                (c[(6 * i) + 5, 0] * t5)),
        (float)((c[(6 * i) + 0, 1] * 1) +
                (c[(6 * i) + 1, 1] * t) +
                (c[(6 * i) + 2, 1] * t2) +
                (c[(6 * i) + 3, 1] * t3) +
                (c[(6 * i) + 4, 1] * t4) +
                (c[(6 * i) + 5, 1] * t5)));
    }

    public IEnumerable<Vector2> GetPositions(double beginTime, double stepInSecond, int count)
    {
        List<Vector2> ret = new(count);
        int i;
        double t2, t3, t4, t5;
        for (i = 0; i < N; i++)
        {
            if (beginTime < T1[i])
                break;
            beginTime -= T1[i];
        }
        for (int j = 0; j < count; j++)
        {
            if (i == N)
            {
                i = N - 1;
                beginTime = T1[i];
            }
            t2 = beginTime * beginTime;
            t3 = t2 * beginTime;
            t4 = t3 * beginTime;
            t5 = t4 * beginTime;
            ret.Add(new(
            (float)((c[(6 * i) + 0, 0] * 1) +
                    (c[(6 * i) + 1, 0] * beginTime) +
                    (c[(6 * i) + 2, 0] * t2) +
                    (c[(6 * i) + 3, 0] * t3) +
                    (c[(6 * i) + 4, 0] * t4) +
                    (c[(6 * i) + 5, 0] * t5)),
            (float)((c[(6 * i) + 0, 1] * 1) +
                    (c[(6 * i) + 1, 1] * beginTime) +
                    (c[(6 * i) + 2, 1] * t2) +
                    (c[(6 * i) + 3, 1] * t3) +
                    (c[(6 * i) + 4, 1] * t4) +
                    (c[(6 * i) + 5, 1] * t5))));
            beginTime += stepInSecond;
            while (i < N && beginTime >= T1[i])
            {
                beginTime -= T1[i];
                ++i;
            }
        }
        return ret;
    }
    public IEnumerable<Vector2> GetVelocitys(double beginTime, double stepInSecond, int count)
    {
        List<Vector2> ret = new(count);
        int i;
        double t2, t3, t4, t5;
        for (i = 0; i < N; i++)
        {
            if (beginTime < T1[i])
                break;
            beginTime -= T1[i];
        }
        for (int j = 0; j < count; j++)
        {
            if (i == N)
            {
                i = N - 1;
                beginTime = T1[i];
            }
            t2 = 2 * beginTime;
            t3 = 3 * t2 * beginTime / 2;
            t4 = 4 * t3 * beginTime / 3;
            t5 = 5 * t4 * beginTime / 4;
            ret.Add(new(
            (float)(
                    (c[(6 * i) + 1, 0] * 1) +
                    (c[(6 * i) + 2, 0] * t2) +
                    (c[(6 * i) + 3, 0] * t3) +
                    (c[(6 * i) + 4, 0] * t4) +
                    (c[(6 * i) + 5, 0] * t5)),
            (float)(
                    (c[(6 * i) + 1, 1] * 1) +
                    (c[(6 * i) + 2, 1] * t2) +
                    (c[(6 * i) + 3, 1] * t3) +
                    (c[(6 * i) + 4, 1] * t4) +
                    (c[(6 * i) + 5, 1] * t5))));
            beginTime += stepInSecond;
            while (i < N && beginTime >= T1[i])
            {
                beginTime -= T1[i];
                ++i;
            }
        }
        return ret;
    }
}