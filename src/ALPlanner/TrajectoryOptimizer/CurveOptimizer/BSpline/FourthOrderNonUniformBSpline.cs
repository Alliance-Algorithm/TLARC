
using Accord.Math;
using Accord.Math.Optimization;
using g4;
using Microsoft.Toolkit.HighPerformance;
using TlarcKernel;
using TlarcKernel.TrajectoryOptimizer.Curves;

namespace ALPlanner.TrajectoryOptimizer.Curves.BSpline;

class FourthOrderNonUniformBSpline : Component, IKOrderBSpline
{
    ControlPointOptimizer controlPointOptimizer;
    private double looseSize = 0.15;
    private double vLimit = 6;
    private double aLimit = 12;
    private double ratioLimit = 1.01;
    private double timeInterval = 0.05f;
    const int order = 4;
    private double[] timeline;
    private double[][] controlPoints = new double[3][];
    private readonly static double[,] M4S = new double[order, order]
    {
        {1, 4,1,0},
        {-3,0,3,0},
        {3,-6,3,0},
        {-1,3,-3,1}
    }.Divide(6);
    private static double[,] H4S = new double[order, order];
    private readonly double[,] M4Data = new double[order, order];
    private int lastTimeIndex = -1;

    public ref double[][] ControlPoint => ref controlPoints;

    private double[,] M4(int i)
    {
        var tmp = Math.Pow(timeline[i + 1] - timeline[i], 2);
        M4Data[0, 0] = tmp / (timeline[i + 1] - timeline[i - 1]) / (timeline[i + 1] - timeline[i - 2]);
        M4Data[0, 2] = tmp / (timeline[i + 2] - timeline[i - 1]) / (timeline[i + 1] - timeline[i - 1]);
        M4Data[1, 2] = 3 * (timeline[i + 1] - timeline[i]) * (timeline[i] - timeline[i - 1]) / (timeline[i + 2] - timeline[i - 1]) / (timeline[i + 1] - timeline[i - 1]);
        M4Data[2, 2] = 3 * tmp / (timeline[i + 2] - timeline[i - 1]) / (timeline[i + 1] - timeline[i - 1]);
        M4Data[3, 3] = tmp / (timeline[i + 3] - timeline[i]) / (timeline[i + 2] - timeline[i]);
        M4Data[3, 2] = -M4Data[2, 2] / 3 - M4Data[3, 3] - tmp / (timeline[i + 2] - timeline[i]) / (timeline[i + 2] - timeline[i - 1]);

        M4Data[1, 0] = -3 * M4Data[0, 0];
        M4Data[2, 0] = -M4Data[1, 0];
        M4Data[3, 0] = -M4Data[0, 0];

        M4Data[0, 1] = 1 - M4Data[0, 0] - M4Data[0, 2];
        M4Data[1, 1] = -M4Data[1, 0] - M4Data[1, 2];
        M4Data[2, 1] = -M4Data[2, 0] - M4Data[2, 2];
        M4Data[3, 1] = M4Data[0, 0] - M4Data[3, 2] - M4Data[3, 3];
        return M4Data;
    }


    public void Construction(IEnumerable<Vector3d> positionList, Vector3dTuple2 HeadTailVelocity, Vector3dTuple2 HeadTailAcceleration)
    {
        double[,] A = new double[(positionList.Count() - 2) * 2 + 6, positionList.Count() + order - 1];
        double[][] B = [
            new double[(positionList.Count() - 2) * 2 + 6],
            new double[(positionList.Count() - 2) * 2 + 6],
            new double[(positionList.Count() - 2) * 2 + 6]
            ];
        double[,] H = new double[positionList.Count() + order - 1, positionList.Count() + order - 1];
        for (int i = 1; i < positionList.Count() - 1; i++)
            for (int j = 0; j < order; j++)
            {
                A[positionList.Count() - 2 + i + 5, i + j] = -M4S[0, j];
                A[i + 5, i + j] = -A[positionList.Count() - 2 + i, i - 1 + j];
            }

        for (int j = 0; j < order; j++)
        {
            A[0, j] = A[1, j + positionList.Count() - 1] = M4S[0, j];
            A[2, j] = A[3, j + positionList.Count() - 1] = M4S[1, j];
            A[4, j] = A[5, j + positionList.Count() - 1] = M4S[2, j];
        }

        int index = 0;
        foreach (var position in positionList)
        {
            if (index == 0)
            {
                B[0][0] = position.x;
                B[1][0] = position.y;
                B[2][0] = position.z;
                continue;
            }
            if (index == positionList.Count() - 1)
            {
                B[0][1] = position.x;
                B[1][1] = position.y;
                B[2][1] = position.z;
                continue;
            }
            B[0][index + 5] = position.x + looseSize;
            B[1][index + 5] = position.y + looseSize;
            B[2][index + 5] = position.z + looseSize;
            B[0][index + positionList.Count() - 2] = -position.x + looseSize;
            B[1][index + positionList.Count() - 2] = -position.y + looseSize;
            B[2][index + positionList.Count() - 2] = -position.z + looseSize;
        }
        B[0][2] = HeadTailVelocity.V0.x;
        B[1][2] = HeadTailVelocity.V0.y;
        B[2][2] = HeadTailVelocity.V0.z;
        B[0][3] = HeadTailVelocity.V1.x;
        B[1][3] = HeadTailVelocity.V1.y;
        B[2][3] = HeadTailVelocity.V1.z;
        B[0][4] = HeadTailAcceleration.V0.x;
        B[1][4] = HeadTailAcceleration.V0.y;
        B[2][4] = HeadTailAcceleration.V0.z;
        B[0][5] = HeadTailAcceleration.V1.x;
        B[1][5] = HeadTailAcceleration.V1.y;
        B[2][5] = HeadTailAcceleration.V1.z;

        var constraints1 = LinearConstraintCollection.Create(A, B[0], 6);
        var constraints2 = LinearConstraintCollection.Create(A, B[1], 6);
        var constraints3 = LinearConstraintCollection.Create(A, B[2], 6);

        for (int i = 0; i < positionList.Count(); i++)
            for (int j = 0; j < order; j++)
                for (int k = 0; k < order; k++)
                    H[i + j, i + k] += H4S[j, k];

        QuadraticObjectiveFunction func = new QuadraticObjectiveFunction(H, null);
        var solver = new GoldfarbIdnani(func, constraints1);
        solver.Minimize();
        controlPoints[0] = solver.Solution;
        solver = new GoldfarbIdnani(func, constraints2);
        solver.Minimize();
        controlPoints[1] = solver.Solution;
        solver = new GoldfarbIdnani(func, constraints3);
        solver.Minimize();
        controlPoints[2] = solver.Solution;

        controlPointOptimizer.Optimize(this);

        ReallocTimeline();
    }

    public Vector3d Value(double time)
    {
        int k = 2;
        time = Math.Max(0, Math.Min(time, timeline[^3]));
        while (timeline[k + 1] < time)
            k++;
        var u = (time - timeline[k]) / (timeline[k + 1] - timeline[k]);
        if (lastTimeIndex != k)
        {
            M4(k);
            lastTimeIndex = k;
        }

        var p = new double[1][] { [1, u, u * u, u * u * u] }.Dot(M4Data);

        return new(p.Dot(controlPoints.Get(0, 0, k, k + 3))[0][0], p.Dot(controlPoints.Get(1, 1, k, k + 3))[0][0], p.Dot(controlPoints.Get(2, 2, k, k + 3))[0][0]);


    }

    public IEnumerable<Vector3d> TrajectoryPoints(double fromWhen, double toWhen, double step)
    {
        List<Vector3d> data = [];
        for (var b = fromWhen; b < toWhen; b += step)
            data.Add(Value(b));
        return data;
    }

    public override void Start()
    {
        for (double u = 0; u < 1; u += 0.1)
        {
            var k = new double[1, 4] { { 0, 0, 2, 6 * u / 100 } }.Dot(M4S);
            H4S = H4S.Add(k.TransposeAndDot(k));
        }
    }

    private void ReallocTimeline()
    {
        lastTimeIndex = -1;
        timeline = new double[controlPoints[0].Length + order - 1];
        timeline[0] = -2 * timeInterval;
        for (int i = 1; i < timeline.Length; i++)
            timeline[i] = timeline[i - 1] + timeInterval;

        while (true)
        {
            var tmpControlPoints = new double[3, controlPoints[0].Length];
            for (int j = 0; j < controlPoints[0].Length - 1; j++)
            {
                tmpControlPoints[0, j] = 3 * (controlPoints[0][j + 1] - controlPoints[0][j]) / (timeline[j + 3] - timeline[j]);
                tmpControlPoints[1, j] = 3 * (controlPoints[1][j + 1] - controlPoints[1][j]) / (timeline[j + 3] - timeline[j]);
                tmpControlPoints[2, j] = 3 * (controlPoints[2][j + 1] - controlPoints[2][j]) / (timeline[j + 3] - timeline[j]);
            }

            double vMax = 0;
            int index = -1;

            for (int k = 2; k <= timeline.Length - 3; k++)
            {
                var m_00 = (timeline[k + 1] - timeline[k]) / (timeline[k + 1] - timeline[k - 1]);
                var m_01 = (timeline[k] - timeline[k - 1]) / (timeline[k + 1] - timeline[k - 1]);

                var v = tmpControlPoints.Get(0, 2, k - 2, k - 1).Dot(new double[,] { { m_00 }, { m_01 } }).Euclidean();
                if (vMax < v)
                {
                    index = k;
                    vMax = v;
                }
            }
            if (vMax < vLimit)
                break;

            var i = index;

            var ratio = Math.Min(ratioLimit, vMax / vLimit) + 1e-4;
            var tOld = timeline[i + 2] - timeline[i - 2];
            var tNew = ratio * tOld;
            var tInt = tNew / 4;

            var head = i - 1;
            var tail = i + 2;

            for (var j = head; j <= tail; j++)
                timeline[j] = tInt + timeline[j - 1];

            for (var j = tail + 1; j < timeline.Length; j++)
                timeline[j] = tNew - tOld + timeline[j];

        }

        while (true)
        {
            var tmpControlPoints = new double[3, controlPoints[0].Length];
            for (int j = 0; j < controlPoints[0].Length - 1; j++)
            {
                tmpControlPoints[0, j] = 3 * (controlPoints[0][j + 1] - controlPoints[0][j]) / (timeline[j + 3] - timeline[j]);
                tmpControlPoints[1, j] = 3 * (controlPoints[1][j + 1] - controlPoints[1][j]) / (timeline[j + 3] - timeline[j]);
                tmpControlPoints[2, j] = 3 * (controlPoints[2][j + 1] - controlPoints[2][j]) / (timeline[j + 3] - timeline[j]);
            }
            for (int j = 0; j < controlPoints[0].Length - 2; j++)
            {
                tmpControlPoints[0, j] = 2 * (controlPoints[0][j + 1] - controlPoints[0][j]) / (timeline[j + 2] - timeline[j]);
                tmpControlPoints[1, j] = 2 * (controlPoints[1][j + 1] - controlPoints[1][j]) / (timeline[j + 2] - timeline[j]);
                tmpControlPoints[2, j] = 2 * (controlPoints[2][j + 1] - controlPoints[2][j]) / (timeline[j + 2] - timeline[j]);
            }

            double aMax = 0;
            int index = -1;

            for (int k = 2; k <= timeline.Length - 3; k++)
            {
                var m_00 = (timeline[k + 1] - timeline[k]) / (timeline[k + 1] - timeline[k - 1]);
                var m_01 = (timeline[k] - timeline[k - 1]) / (timeline[k + 1] - timeline[k - 1]);

                var a = tmpControlPoints.Get(0, 2, k - 2, k - 1).Dot(new double[,] { { m_00 }, { m_01 } }).Euclidean();
                if (aMax < a)
                {
                    index = k;
                    aMax = a;
                }
            }
            if (aMax < aLimit)
                break;

            var i = index;

            var ratio = Math.Min(ratioLimit, aMax / aLimit) + 1e-4;
            var tOld = timeline[i + 2] - timeline[i - 2];
            var tNew = ratio * tOld;
            var tInt = tNew / 4;

            var head = i - 1;
            var tail = i + 2;

            for (var j = head; j <= tail; j++)
                timeline[j] = tInt + timeline[j - 1];

            for (var j = tail + 1; j < timeline.Length; j++)
                timeline[j] = tNew - tOld + timeline[j];
        }
    }

}