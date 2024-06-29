
using MathNet.Numerics.LinearAlgebra.Single;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Threading;
namespace AllianceDM.ALPlanner;


public class NonUniformBSpline(float limitVelocity, float limitAccelerate, float limitRatio)
{
    /// <summary>
    /// 运动曲线控制点
    /// </summary>
    public DenseMatrix controlPoints_;
    /// <summary>
    /// 时间曲线控制点
    /// </summary>
    public DenseMatrix timeControlPoints_;
    /// <summary>
    /// 路点个数
    /// </summary>
    public int n_;
    /// <summary>
    /// K-1
    /// </summary>
    // private int p_ = 3;
    /// <summary>
    /// 时间向量
    /// </summary>
    private DenseVector t_;
    /// <summary>
    /// M^k 矩阵,for timeline
    /// </summary>
    private DenseMatrix m_tk;
    /// <summary>
    /// M^k 矩阵
    /// </summary>
    private DenseMatrix m_k;

    private float _limitVelocity = limitVelocity;
    private float _limitAccelerate = limitAccelerate;
    private float _limitRatio = limitRatio;

    /// <summary>
    /// 生成控制点
    /// </summary>
    /// <param name="WayPoints">路点约束</param>
    /// <param name="StartEndDerivative">首尾速度加速度约束</param>
    /// <param name="K">K阶BSpline</param>
    internal void ParametersToControlPoints(List<Vector2> WayPoints, List<Vector2> StartEndDerivative)
    {
        n_ = WayPoints.Count;
        if (m_k == null)
        {
            m_k = new DenseMatrix(4, 4);
            m_k.SetRow(0, new float[] { 1 / 6f, 4 / 6f, 1 / 6f, 0 });
            m_k.SetRow(1, new float[] { -3 / 6f, 0, 3 / 6f, 0 });
            m_k.SetRow(2, new float[] { 3 / 6f, -6 / 6f, 3 / 6f, 0 });
            m_k.SetRow(3, new float[] { -1 / 6f, 3 / 6f, -3 / 6f, 1 / 6f });
        }
        DenseMatrix A = new(n_ + 3, n_ + 3);
        for (int i = 0; i < n_; i++)
            A.SetSubMatrix(i, 1, i + 0, 3, m_k.SubMatrix(0, 1, 0, 3));
        A.SetSubMatrix(n_, 1, 0, 3, m_k.SubMatrix(1, 1, 0, 3));
        A.SetSubMatrix(n_ + 1, 1, n_ - 1, 3, m_k.SubMatrix(1, 1, 0, 3));
        A.SetSubMatrix(n_ + 2, 1, n_, 3, m_k.SubMatrix(1, 1, 0, 3));
        // A.SetSubMatrix(n_ + 4, 1, n_ + 0, 3, m_k.SubMatrix(2, 1, 0, 3));
        // A.SetSubMatrix(n_ + 5, 1, n_ + 1, 3, m_k.SubMatrix(2, 1, 0, 3));

        DenseMatrix b = new DenseMatrix(n_ + 3, 3);
        // b.SetRow(2, new float[] { StartEndDerivative[2].X, StartEndDerivative[2].Y, 0 });
        for (int i = 0; i < n_; i++)
            b.SetRow(i, new float[] { WayPoints[i].X, WayPoints[i].Y, 0 });
        b.SetRow(n_, new float[] { StartEndDerivative[0].X, StartEndDerivative[0].Y, 0 });
        b.SetRow(n_ + 1, new float[] { StartEndDerivative[1].X, StartEndDerivative[1].Y, 0 });

        controlPoints_ = (DenseMatrix)A.Solve(b);
    }
    public void BuildTimeLine(float TimeInterval = 0.1f)
    {
        DenseVector timeDensity = new DenseVector(n_);
        for (int i = 0; i < n_; i++)
            timeDensity[i] = TimeInterval;
        timeDensity[0] = timeDensity[n_ - 1] = 1;
        DenseMatrix u = new DenseMatrix(1, 4);
        u.SetRow(0, new float[] { 0, 1, 0, 0 });
        for (int i = 0; i < n_; i++)
        {
            var vel = u * m_k * controlPoints_.SubMatrix(i, 4, 0, 2);
            var vel_t = new Vector2(vel[0, 0] / timeDensity[i], vel[0, 1] / timeDensity[i]).Length();
            if (vel_t > _limitVelocity)
                timeDensity[i] *= vel_t / _limitVelocity;
        }
        u.SetRow(0, new float[] { 0, 0, 2, 0 });
        for (int i = 0; i < n_; i++)
        {
            var acc = u * m_k * controlPoints_.SubMatrix(i, 4, 0, 2);
            var acc_t = new Vector2(acc[0, 0] / timeDensity[i] / timeDensity[i], acc[0, 1] / timeDensity[i] / timeDensity[i]).Length();
            if (acc_t > _limitAccelerate)
                timeDensity[i] *= (float)Math.Sqrt(acc_t / _limitAccelerate);
        }


        m_tk = new DenseMatrix(3);
        m_tk.SetRow(0, new float[] { 1 / 2f, 1 / 2f, 0 });
        m_tk.SetRow(1, new float[] { -2 / 2f, 2 / 2f, 0 });
        m_tk.SetRow(2, new float[] { 1 / 2f, -2 / 2f, 1 / 2f });

        DenseMatrix A = new DenseMatrix(n_ + 2, n_ + 2);
        DenseMatrix b = new DenseMatrix(n_ + 2, 1);
        A.SetSubMatrix(0, 2, 0, 2, m_tk);
        b[1, 0] = timeDensity[0];
        for (int i = 1; i < n_; i++)
        {
            A.SetSubMatrix(i + 1, 1, i, 2, m_tk.SubMatrix(1, 1, 0, 2));
            b[i + 1, 0] = timeDensity[i];
        }
        A.SetSubMatrix(n_ + 1, 1, n_, 2, m_tk.SubMatrix(1, 1, 0, 2));

        timeControlPoints_ = (DenseMatrix)A.Solve(b);


        A = new DenseMatrix(n_, n_ + 2);
        for (int i = 0; i < n_; i++)
        {
            A.SetSubMatrix(i, 1, i, 2, m_tk.SubMatrix(0, 1, 0, 2));
        }
        t_ = (DenseVector)(A * timeControlPoints_).Transpose().Row(0);
    }

    public (int, bool) Check(float t, GlobalESDFMap costMap, Vector2 position)
    {
        t = Math.Min(Math.Max(t, t_[0]), t_[n_ - 1]);
        int k = 0;
        while (t_[k + 1] < t) k++;
        var u = (t - t_[k]) / (t_[k + 1] - t_[k]);
        var temp = CalcPosition(u, k);
        if ((temp - position).Length() > 0.6f)
            return (k, false);
        for (t += 0.1f; t < t_[k + 1]; t += 0.1f)
        {
            u = (t - t_[k]) / (t_[k + 1] - t_[k]);
            temp = CalcPosition(u, k);
            var t2 = costMap.Vector2ToXY(temp);
            if (costMap[t2.x, t2.y] <= 0)
                return (k, false);
        }
        return (k, true);
    }
    public List<Vector3> GetPath()
    {
        var ret = new List<Vector3>();
        for (float t = 0; t < t_[n_ - 1]; t += 0.1f)
        {
            t = Math.Min(Math.Max(t, t_[0]), t_[n_ - 1]);
            int k = 0;
            while (t_[k + 1] < t) k++;

            var u = (t - t_[k]) / (t_[k + 1] - t_[k]);
            ret.Add(new(CalcPosition(u, k), 0));
        }
        return ret;
    }
    private Vector2 CalcPosition(float U, int N)
    {
        var u_ = new DenseMatrix(1, 4);
        u_.SetRow(0, new float[] { 1, U, U * U, U * U * U });
        var ret = u_ * m_k * controlPoints_.SubMatrix(N, 4, 0, 2);
        return new(ret[0, 0], ret[0, 1]);
    }
    internal Vector3 GetVelocity(float t)
    {
        t = Math.Min(Math.Max(t, t_[0]), t_[n_ - 1]);
        int k = 0;
        while (t_[k + 1] < t) k++;

        var u = (t - t_[k]) / (t_[k + 1] - t_[k]);
        var ret = CalcVelocity(u, k);
        return new Vector3(ret, 0);
    }

    private Vector2 CalcVelocity(float U, int N)
    {
        var u_ = new DenseMatrix(1, 4);
        u_.SetRow(0, new float[] { 0, 1, 2 * U, 3 * U * U });
        var ret = u_ * m_k * controlPoints_.SubMatrix(N, 4, 0, 3);
        var timeDensity = u_.SubMatrix(0, 1, 0, 3) * m_tk * timeControlPoints_.SubMatrix(N, 3, 0, 1);
        timeDensity[0, 0] = t_[N + 1] - t_[N];
        ret /= timeDensity[0, 0];
        return new(ret[0, 0], ret[0, 1]);
    }


    internal Vector3 GetAcceleration(float t)
    {
        t = Math.Min(Math.Max(t, t_[0]), t_[n_ - 1]);
        int k = 0;
        while (t_[k + 1] < t) k++;

        var u = (t - t_[k]) / (t_[k + 1] - t_[k]);
        var ret = CalcAcceleration(u, k);
        return new Vector3(ret, 0);
    }
    private Vector2 CalcAcceleration(float U, int N)
    {
        var u_ = new DenseMatrix(1, 4);
        u_.SetRow(0, new float[] { 0, 0, 2, 6 * U });
        var ret = u_ * m_k * controlPoints_.SubMatrix(N, 4, 0, 3);
        var timeDensity = u_.SubMatrix(0, 1, 0, 3) * m_tk * timeControlPoints_.SubMatrix(N, 3, 0, 1);
        timeDensity[0, 0] = t_[N + 1] - t_[N];
        ret /= timeDensity[0, 0] * timeDensity[0, 0];
        return new(ret[0, 0], ret[0, 1]);
    }
}
