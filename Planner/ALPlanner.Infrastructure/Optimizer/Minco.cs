/*
使用N = 7 k = 3(jerk)的多项式
*/
using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Accord.Math;
using Accord.Math.Optimization;
using Kernel.DataInterfaces.Navigation;

namespace ALPlanner.Infrastructure.Optimizer;

struct PolynomialPara
{
    public struct Vector2d { public double x; public double y; }

    public Vector2d p0;
    public Vector2d v0;
    public Vector2d a0;
    public Vector2d j0;

    public Vector2d pt;
    public Vector2d vt;
    public Vector2d at;
    public Vector2d jt;

    public double t;

    public readonly void ToMat(in double[,] doubles)
    {
        doubles[0, 0] = p0.x;
        doubles[0, 1] = p0.y;
        doubles[1, 0] = v0.x;
        doubles[1, 1] = v0.y;
        doubles[2, 0] = a0.x;
        doubles[2, 1] = a0.y;
        doubles[3, 0] = j0.x;
        doubles[3, 1] = j0.y;
        doubles[4, 0] = pt.x;
        doubles[4, 1] = pt.y;
        doubles[5, 0] = vt.x;
        doubles[5, 1] = vt.y;
        doubles[6, 0] = at.x;
        doubles[6, 1] = at.y;
        doubles[7, 0] = jt.x;
        doubles[7, 1] = jt.y;
    }
}
class PolynomialSegment(IObstacle obstacle, float[] qRef)
{
    public double _ti = 0;
    double _lastTi = -1;
    readonly double[,] _A = new double[8, 8];
    readonly double[,] _dA_dt = new double[8, 8];
    readonly double[,] _Q = new double[8, 8];
    readonly double[,] _D = new double[8, 2];
    readonly double[,] _dQ_dt = new double[8, 8];
    readonly double[] _gradient = new double[9];
    double[] _obsG = [];
    double[,] X = { };
    readonly double[] _T = new double[8];
    double _j_snap = 0;
    double _j_soft = 0;
    void AInverse()
    {
        _A[0, 0] = 1;
        _A[1, 1] = 1;
        _A[2, 2] = 2;
        _A[3, 3] = 6;

        for (int k = 0; k < 8; k++)
        {
            _A[4, k] = Math.Pow(_ti, k);
            _A[5, k] = _A[4, k] * k / _ti;
            _A[6, k] = _A[5, k] * (k - 1) / _ti;
            _A[7, k] = _A[6, k] * (k - 2) / _ti;
        }
        var T = _ti;
        _dA_dt[4, 1] = 1;
        _dA_dt[4, 2] = 2 * T;
        _dA_dt[4, 3] = 3 * T * T;
        _dA_dt[4, 4] = 4 * T * T * T;
        _dA_dt[4, 5] = 5 * Math.Pow(T, 4);
        _dA_dt[4, 6] = 6 * Math.Pow(T, 5);
        _dA_dt[4, 7] = 7 * Math.Pow(T, 6);

        // 第 6 行: [0, 0, 2, 6T, 12T^2, 20T^3, 30T^4, 42T^5]
        _dA_dt[5, 2] = 2;
        _dA_dt[5, 3] = 6 * T;
        _dA_dt[5, 4] = 12 * T * T;
        _dA_dt[5, 5] = 20 * T * T * T;
        _dA_dt[5, 6] = 30 * Math.Pow(T, 4);
        _dA_dt[5, 7] = 42 * Math.Pow(T, 5);

        // 第 7 行: [0, 0, 0, 6, 24T, 60T^2, 120T^3, 210T^4]
        _dA_dt[6, 3] = 6;
        _dA_dt[6, 4] = 24 * T;
        _dA_dt[6, 5] = 60 * T * T;
        _dA_dt[6, 6] = 120 * T * T * T;
        _dA_dt[6, 7] = 210 * Math.Pow(T, 4);

        // 第 8 行: [0, 0, 0, 0, 24, 120T, 360T^2, 840T^3]
        _dA_dt[7, 4] = 24;
        _dA_dt[7, 5] = 120 * T;
        _dA_dt[7, 6] = 360 * T * T;
        _dA_dt[7, 7] = 840 * T * T * T;
    }
    void Q()
    {
        _Q[4, 4] = 24;
        _Q[5, 5] = 60 * _ti;
        _Q[6, 6] = 120 * _ti * _ti;
        _dQ_dt[5, 5] = 60;
        _dQ_dt[6, 6] = 240 * _ti;
    }
    public void Update(PolynomialPara para)
    {
        para.ToMat(_D);
        var t = para.t;

        if (t == _lastTi)
            return;
        _ti = t;
        AInverse();
        Q();

        X = _A.Solve(_D);
        _j_snap = X.Dot(_Q).Dot(X).Trace();
        var dJ_dX = _Q.Dot(X).Multiply(2);
        var lambda = _A.Transpose().Solve(dJ_dX);
        double gradient_t = lambda.Dot(_dA_dt).Dot(X).Trace()
                        + X.Dot(_dQ_dt).Dot(X).Trace();

        var QX = _Q.Dot(X);
        var Y = _A.Transpose().Solve(QX);
        var dJ_smooth_dq = Y.Multiply(2);

        _j_soft = (X[0, 0] / 2 - qRef[0]) * X[0, 0] + (X[0, 4] / 2 - qRef[1]) * X[0, 4] +
                    (X[1, 0] / 2 - qRef[2]) * X[1, 0] + (X[1, 4] / 2 - qRef[3]) * X[1, 4];
        double[] dJ_soft_dq = [_D[0, 0] - qRef[0], _D[0, 4] - qRef[1], _D[1, 0] - qRef[2], _D[1, 4] - qRef[3]];

        for (int i = 0; i < 8; i++)
        {
            _gradient[i] = dJ_smooth_dq[i, 0];
            _gradient[i + 8] = dJ_smooth_dq[i, 1];
        }
        _gradient[17] = gradient_t;
        _gradient[0] += dJ_soft_dq[0];
        _gradient[4] += dJ_soft_dq[1];
        _gradient[8] += dJ_soft_dq[2];
        _gradient[12] += dJ_soft_dq[3];
        _lastTi = _ti;

    }

    public Vector2 GetPosition(double t)
    {
        double tp = 1;
        for (int i = 0; i < 8; ++i)
        {
            tp *= t;
            _T[i] = tp;
        }
        return Unsafe.As<Vector2[]>(_T.Dot(X))[0];
    }


    public double J => _j_snap + _j_soft;
    public void G(in Span<double> outG) => _gradient.CopyTo(outG);
}

public class Minco(IObstacle obstacle, string id) : ITrajectory2D
{
    int c;
    readonly List<PolynomialSegment> segments = [];

    public DateTime FromWhen { get; private set; }

    public DateTime ToWhen { get; private set; }
    Vector2 _tail;

    public string Identifier => id;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void SetSegments(PolynomialPara[] solution)
    {
        for (int i = 0; i < solution.Length; ++i)
            segments[i].Update(solution[i]);
    }

    double Function(double[] x)
    {
        SetSegments(Unsafe.As<PolynomialPara[]>(x));
        return segments.Sum(p => p.J);
    }
    double[] Gradient(double[] x)
    {
        double[] g = new double[x.Length];
        SetSegments(Unsafe.As<PolynomialPara[]>(x));
        Parallel.For(0, c,
        i => segments[i].G(g.AsSpan()[(i * 17)..((i + 1) * 17)])
        );
        return g;
    }


    public ITrajectory2D Optimize(Vector2[] path)
    {
        c = path.Length;
        _tail = path[^1];
        FromWhen = DateTime.UtcNow;
        segments.Clear();
        for (int i = 1; i < c; ++i)
            segments.Add(new(obstacle, [path[i - 1].X, path[i].X, path[i - 1].Y, path[i].Y]));

        var function = new NonlinearObjectiveFunction(
            numberOfVariables: c * (8 * 2 + 1),
            function: Function,
            gradient: Gradient
        );

        BroydenFletcherGoldfarbShanno optimizer = new(function);

        if (!optimizer.Minimize())
            segments.Clear();
        else
        {
            SetSegments(Unsafe.As<PolynomialPara[]>(optimizer.Solution));
            ToWhen = FromWhen + TimeSpan.FromSeconds(Unsafe.As<PolynomialPara[]>(optimizer.Solution).Sum(x => x.t));
        }
        return this;
    }

    public Vector2 GetPosition(DateTime time)
    {
        if (segments.Count == 0)
            throw new Exception("Failed optimize with MINCO, pls check first");

        if (time < ToWhen)
        {
            PolynomialSegment segment = segments[0];
            double second = (time - FromWhen).TotalSeconds;
            foreach (var s in segments)
            {
                if (second >= s._ti)
                {
                    second -= s._ti;
                    continue;
                }
                segment = s;
                break;
            }
            return segment.GetPosition(second);
        }
        return _tail;
    }

    public IEnumerable<Vector2> GetPositionArray(DateTime beginTime, double stepInSecond, int count)
    {
        List<Vector2> data = new(count);
        var time = beginTime;
        int index = 0;
        double second = 0;
        if (time < ToWhen)
        {
            second = (time - FromWhen).TotalSeconds;
            foreach (var s in segments)
            {
                if (second < s._ti)
                    break;
                index++;
                second -= s._ti;
                continue;
            }
        }

        for (int i = 0; i < count; ++i)
        {
            if (time < ToWhen)
                data.Add(segments[index].GetPosition(second));
            else data.Add(_tail);
            time += TimeSpan.FromSeconds(stepInSecond);
            second += stepInSecond;
            if (second >= segments[index]._ti)
                index++;
        }
        return data;
    }
}