/*
使用N = 7 s = 3(jerk)的多项式
*/
using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using CommunityToolkit.HighPerformance;
using Kernel.DataInterfaces.Navigation;
using Vectorf = NumFlat.Vec<double>;
using Matrixf = NumFlat.Mat<double>;
using Kernel.DataInterfaces.Constraints;
using NumFlat;
using Kernel.DataInterfaces.Visualization;

namespace ALPlanner.Infrastructure.Optimizer;

public unsafe class Minco<T> where T : IConstraint
{
    internal readonly int N;
    internal readonly Matrixf A;
    internal readonly Vectorf T1;
    internal readonly Vectorf T2;
    internal readonly Vectorf T3;
    internal readonly Vectorf T4;
    internal readonly Vectorf T5;
    internal readonly Matrixf c;
    internal readonly Matrixf G;
    internal readonly Matrixf gC;
    internal readonly Matrixf gQ;
    internal readonly Vectorf gT;
    internal readonly Vectorf gKesi;
    internal readonly Vectorf gd;
    readonly PolynomialTraj poly;
    readonly MincoTraj minco;
    readonly IDiffeomorphism diffeomorphism;
    readonly int[] ipiv;

    readonly public Vector2[] _headPVA;
    readonly public Vector2[] _tailPVA;

    readonly Matrixf Q;

    readonly T[] _obstacles;

    public double TotalSecond { get; private set; }

    const int S = Minco.S;

    public Minco(in int N, in T[] obstacles)
    {
        this.N = N;
        this._headPVA = new Vector2[3];
        this._tailPVA = new Vector2[3];
        this.A = new Matrixf(2 * S * N, 2 * S * N);
        this.T1 = new Vectorf(N);
        this.T2 = new Vectorf(N);
        this.T3 = new Vectorf(N);
        this.T4 = new Vectorf(N);
        this.T5 = new Vectorf(N);
        this.c = new Matrixf(2 * S * N, 2);
        this.gC = new Matrixf(2 * S * N, 2);
        this.G = new Matrixf(2 * S * N, 2);
        this.gQ = new Matrixf(N - 1, 2);
        this.gd = new Vectorf((N - 1) * 2 - obstacles.Length + 1 + N);
        this.gT = gd[..N];
        this.gKesi = gd[N..];
        this.Q = new Matrixf(N - 1, 2);
        this._obstacles = obstacles;
        ipiv = new int[A.RowCount];
        poly = new PolynomialTraj(N, c, T1, T2, T3, T4, T5, gT, gC);
        minco = new MincoTraj(c, N, T1, T2, T3, T4, T5, A, _headPVA, _tailPVA, ipiv);
        diffeomorphism = DiffeomorphismFactory.Build(T1, Q, gQ, gKesi, _obstacles);
    }



    public void Generate(in Vectorf tau, Vectorf kesi)
    {
        gT.Clear();
        gQ.Clear();
        gC.Clear();


        diffeomorphism.VTToRT(tau);
        diffeomorphism.KesiToQ(kesi);
        minco.Generate(Q);
        poly.Calculate();
        TotalSecond = T1.Sum();
    }

    public double GetJ() =>
        diffeomorphism.AddJCostT() +
        poly.AddJCostJerk() + poly.Costs;


    /// <summary>
    /// <para>@param:[in,out] gT. In:\frac{\partial K}{\partial T} Out:\frac{\partial W}{\partial T}.</para>
    /// @param:[out] gQ. \frac{\partial W}{\partial q}
    /// </summary>
    /// <param name="gT"></param>
    /// <param name="gQ"></param>
    public Vectorf GetGradient(Vectorf kesi)
    {
        poly.AddGradJbyC();
        poly.AddGradJbyT();

        fixed (int* ipivPtr = ipiv)
        fixed (double* APtr = A.Memory.Span)
        fixed (double* bPtr = gC.Memory.Span)
            unsafe
            {
                int n = A.ColCount;
                int nrhs = gC.ColCount;
                int info;
                // 求解 AX = B
                byte trans = 84;

                if (BlasSharp.OpenBlas.NativeMethods.dgetrs_(&trans, &n, &nrhs, APtr, &n, ipivPtr, bPtr, &n, &info, 0) != 0)
                {
                    throw new Exception($"求解失败，info = {info}");
                }

            }



        // Given G, \frac{\partial K}{\partial T} get the \frac{\partial W}{\partial T}.
        minco.AddPropCtoT(gC, gT);
        // Given G, get the \frac{\partial W}{\partial q}
        minco.AddPropCtoP(gC, gQ);

        diffeomorphism.VirtualTGrad(gT, gT);
        diffeomorphism.AddGradQByKesi(kesi);
        return gd;
    }

    public Minco Record => new(N, T1, c, TotalSecond, _headPVA, _tailPVA);
}
public readonly record struct Minco(in int N, in Vectorf T1, in Matrixf C, in double TotalSecond, in Vector2[] Header, in Vector2[] Tail)
{
    public const int S = 3;
    public readonly Vector2 GetPosition(double t)
    {
        if (t <= 0) return Header[0];
        else if (t >= TotalSecond) return Tail[0];
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
        (float)((C[(6 * i) + 0, 0] * 1) +
                (C[(6 * i) + 1, 0] * t) +
                (C[(6 * i) + 2, 0] * t2) +
                (C[(6 * i) + 3, 0] * t3) +
                (C[(6 * i) + 4, 0] * t4) +
                (C[(6 * i) + 5, 0] * t5)),
        (float)((C[(6 * i) + 0, 1] * 1) +
                (C[(6 * i) + 1, 1] * t) +
                (C[(6 * i) + 2, 1] * t2) +
                (C[(6 * i) + 3, 1] * t3) +
                (C[(6 * i) + 4, 1] * t4) +
                (C[(6 * i) + 5, 1] * t5)));
    }
    public readonly Vector2 GetVelocity(double t)
    {
        if (t <= 0) return Header[1];
        else if (t >= TotalSecond) return Tail[1];
        double t2, t3, t4, t5;
        int i;
        for (i = 0; i < N; i++)
        {
            if (t < T1[i])
                break;
            t -= T1[i];
        }
        t2 = 2 * t;
        t3 = 3 / 2 * t2 * t;
        t4 = 4 / 3 * t3 * t;
        t5 = 5 / 4 * t4 * t;
        return new(
        (float)((C[(6 * i) + 0, 0] * 0) +
                (C[(6 * i) + 1, 0] * 1) +
                (C[(6 * i) + 2, 0] * t2) +
                (C[(6 * i) + 3, 0] * t3) +
                (C[(6 * i) + 4, 0] * t4) +
                (C[(6 * i) + 5, 0] * t5)),
        (float)((C[(6 * i) + 0, 1] * 0) +
                (C[(6 * i) + 1, 1] * 1) +
                (C[(6 * i) + 2, 1] * t2) +
                (C[(6 * i) + 3, 1] * t3) +
                (C[(6 * i) + 4, 1] * t4) +
                (C[(6 * i) + 5, 1] * t5)));
    }
    public readonly Vector2 GetAccelerate(double t)
    {
        if (t <= 0) return Header[2];
        else if (t >= TotalSecond) return Tail[2];
        double t2, t3, t4, t5;
        int i;
        for (i = 0; i < N; i++)
        {
            if (t < T1[i])
                break;
            t -= T1[i];
        }
        t2 = 2;
        t3 = 3 * 2 * t;
        t4 = 4 * 3 * t * t;
        t5 = 5 * 4 * t * t * t;
        return new(
        (float)((C[(6 * i) + 0, 0] * 0) +
                (C[(6 * i) + 1, 0] * 0) +
                (C[(6 * i) + 2, 0] * t2) +
                (C[(6 * i) + 3, 0] * t3) +
                (C[(6 * i) + 4, 0] * t4) +
                (C[(6 * i) + 5, 0] * t5)),
        (float)((C[(6 * i) + 0, 1] * 0) +
                (C[(6 * i) + 1, 1] * 0) +
                (C[(6 * i) + 2, 1] * t2) +
                (C[(6 * i) + 3, 1] * t3) +
                (C[(6 * i) + 4, 1] * t4) +
                (C[(6 * i) + 5, 1] * t5)));
    }
    public readonly IEnumerable<Vector2> GetControlPoints()
    {
        List<Vector2> ret = new(N - 1);
        for (int i = 1; i < N; i++)
            ret.Add(new(
            (float)(C[(6 * i) + 0, 0] * 1),
            (float)(C[(6 * i) + 0, 1] * 1)));
        return ret;
    }
    public readonly IEnumerable<Vector2> GetPositions(double beginTime, double stepInSecond, int count)
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
            if (beginTime <= 0) ret.Add(Header[0]);
            else if (beginTime >= TotalSecond) ret.Add(Tail[0]);
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
            (float)((C[(6 * i) + 0, 0] * 1) +
                    (C[(6 * i) + 1, 0] * beginTime) +
                    (C[(6 * i) + 2, 0] * t2) +
                    (C[(6 * i) + 3, 0] * t3) +
                    (C[(6 * i) + 4, 0] * t4) +
                    (C[(6 * i) + 5, 0] * t5)),
            (float)((C[(6 * i) + 0, 1] * 1) +
                    (C[(6 * i) + 1, 1] * beginTime) +
                    (C[(6 * i) + 2, 1] * t2) +
                    (C[(6 * i) + 3, 1] * t3) +
                    (C[(6 * i) + 4, 1] * t4) +
                    (C[(6 * i) + 5, 1] * t5))));
            beginTime += stepInSecond;
            while (i < N && beginTime >= T1[i])
            {
                beginTime -= T1[i];
                ++i;
            }
        }
        return ret;
    }
    public readonly IEnumerable<Vector2> GetVelocitys(double beginTime, double stepInSecond, int count)
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
            if (beginTime <= 0) ret.Add(Header[0]);
            else if (beginTime >= TotalSecond) ret.Add(Tail[0]);
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
                    (C[(6 * i) + 1, 0] * 1) +
                    (C[(6 * i) + 2, 0] * t2) +
                    (C[(6 * i) + 3, 0] * t3) +
                    (C[(6 * i) + 4, 0] * t4) +
                    (C[(6 * i) + 5, 0] * t5)),
            (float)(
                    (C[(6 * i) + 1, 1] * 1) +
                    (C[(6 * i) + 2, 1] * t2) +
                    (C[(6 * i) + 3, 1] * t3) +
                    (C[(6 * i) + 4, 1] * t4) +
                    (C[(6 * i) + 5, 1] * t5))));
            beginTime += stepInSecond;
            while (i < N && beginTime >= T1[i])
            {
                beginTime -= T1[i];
                ++i;
            }
        }
        return ret;
    }
    public readonly IEnumerable<Vector2> GetAccelerates(double beginTime, double stepInSecond, int count)
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
            if (beginTime <= 0) ret.Add(Header[0]);
            else if (beginTime >= TotalSecond) ret.Add(Tail[0]);
            if (i == N)
            {
                i = N - 1;
                beginTime = T1[i];
            }
            t2 = 2;
            t3 = 6 * beginTime;
            t4 = 12 * beginTime * beginTime;
            t5 = 20 * beginTime * beginTime * beginTime;
            ret.Add(new(
            (float)(
                    (C[(6 * i) + 2, 0] * t2) +
                    (C[(6 * i) + 3, 0] * t3) +
                    (C[(6 * i) + 4, 0] * t4) +
                    (C[(6 * i) + 5, 0] * t5)),
            (float)(
                    (C[(6 * i) + 2, 1] * t2) +
                    (C[(6 * i) + 3, 1] * t3) +
                    (C[(6 * i) + 4, 1] * t4) +
                    (C[(6 * i) + 5, 1] * t5))));
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