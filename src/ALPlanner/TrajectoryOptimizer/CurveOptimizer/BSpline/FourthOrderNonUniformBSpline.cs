using Accord.Math.Optimization;
using Microsoft.Toolkit.HighPerformance;
using TlarcKernel;
using TlarcKernel.TrajectoryOptimizer.Curves;
using TlarcKernel.TrajectoryOptimizer.Optimizer;

namespace ALPlanner.TrajectoryOptimizer.Curves.BSpline;

class FourthOrderNonUniformBSpline : Component, IKOrderBSpline
{
    public DateTime constructTime;
    public double[] controlPointsX = [];
    public double[] controlPointsY = [];
    public double[] controlPointsZ = [];
    public double[] timeline = [];

    [ComponentReferenceFiled]
    IOptimizer controlPointOptimizer;
    public FourthOrderNonUniformBSpline()
    {

    }

    public FourthOrderNonUniformBSpline(double looseSize = 0.15, double vLimit = 6, double aLimit = 12, double ratioLimit = 1.01, double timeInterval = 0.05f)
    {
        _looseSize = looseSize;
        _vLimit = vLimit;
        _aLimit = aLimit;
        _ratioLimit = ratioLimit;
        _timeInterval = timeInterval;
    }

    public double MaxTime => timeline[^5];
    private double _looseSize = 0.15;
    private double _vLimit = 2;
    private double _aLimit = 0.5;
    private double _ratioLimit = 1.01;
    private double _timeInterval = 0.05f;
    const int order = 4;
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

    public double[][] ControlPoint => [controlPointsX, controlPointsY, controlPointsZ];

    public Vector3d Position
    {
        get
        {
            return Value((DateTime.Now - constructTime).Duration().TotalSeconds);
        }
    }

    public DateTime ConstructTime => constructTime;

    private double[,] M4(int i)
    {
        var tmp = Math.Pow(timeline[i + 1] - timeline[i], 2);

        M4Data[0, 0] = tmp / (timeline[i + 1] - timeline[i - 1]) / (timeline[i + 1] - timeline[i - 2]);
        M4Data[0, 2] = Math.Pow(timeline[i] - timeline[i - 1], 2) / (timeline[i + 2] - timeline[i - 1]) / (timeline[i + 1] - timeline[i - 1]);
        M4Data[1, 2] = 3 * (timeline[i + 1] - timeline[i]) * (timeline[i] - timeline[i - 1]) / (timeline[i + 2] - timeline[i - 1]) / (timeline[i + 1] - timeline[i - 1]);
        M4Data[2, 2] = 3 * tmp / (timeline[i + 2] - timeline[i - 1]) / (timeline[i + 1] - timeline[i - 1]);
        M4Data[3, 3] = tmp / (timeline[i + 3] - timeline[i]) / (timeline[i + 2] - timeline[i]);
        M4Data[3, 2] = (-M4Data[2, 2] / 3) - M4Data[3, 3] - (tmp / (timeline[i + 2] - timeline[i]) / (timeline[i + 2] - timeline[i - 1]));

        M4Data[1, 0] = -3 * M4Data[0, 0];
        M4Data[2, 0] = -M4Data[1, 0];
        M4Data[3, 0] = -M4Data[0, 0];

        M4Data[0, 1] = 1 - M4Data[0, 0] - M4Data[0, 2];
        M4Data[1, 1] = -M4Data[1, 0] - M4Data[1, 2];
        M4Data[2, 1] = -M4Data[2, 0] - M4Data[2, 2];
        M4Data[3, 1] = M4Data[0, 0] - M4Data[3, 2] - M4Data[3, 3];

        M4Data[0, 3] = 0;
        M4Data[1, 3] = 0;
        M4Data[2, 3] = 0;
        return M4Data;
    }
    public LinearConstraintCollection CreateConstraints(double[,] a, double[] b)
    {
        int length = a.GetLength(1);
        int length2 = b.Length;
        List<LinearConstraint> array = new();
        for (int i = 0; i < length2; i++)
        {
            if (i > length2 - 3)
                continue;
            else
             if (i >= length2 - 5 || i == 0)
            {
                var constraint = new LinearConstraint(length);
                a.GetRow(i, constraint.CombinedAs);
                constraint.ShouldBe = ConstraintType.EqualTo;
                constraint.Value = b[i];
                array.Add(constraint);
            }
            else
            {
                var constraint1 = new LinearConstraint(length);
                a.GetRow(i, constraint1.CombinedAs);
                var constraint2 = new LinearConstraint(length);
                a.GetRow(i, constraint2.CombinedAs);
                constraint1.ShouldBe = ConstraintType.GreaterThanOrEqualTo;
                constraint2.ShouldBe = ConstraintType.LesserThanOrEqualTo;
                constraint1.Value = b[i] - _looseSize;
                constraint2.Value = b[i] + _looseSize;
                array.Add(constraint1);
                array.Add(constraint2);
            }
        }

        return new LinearConstraintCollection(array);
    }
    public void Construction(Vector3d[] positionList, Vector3dTuple2 HeadTailVelocity, Vector3dTuple2 HeadTailAcceleration)
    {
        if (positionList.Length == 1)
        {
            TlarcSystem.LogError("No Enough Point");
            return;
        }
        constructTime = DateTime.Now;

        double[,] A = new double[positionList.Length + 4, positionList.Length + 4 - 1];
        double[][] B = [
            new double[positionList.Length + 4],
            new double[positionList.Length + 4],
            new double[positionList.Length + 4]
            ];


        for (int i = 0; i < positionList.Length; i++)
            for (int j = 0; j < 4; j++)
                A[i, i + j] = M4S[0, j];

        for (int j = 0; j < 4; j++)
        {
            A[positionList.Length, j] = M4S[1, j];
            A[positionList.Length + 1, j + positionList.Length - 1] = M4S[1, j];
            A[positionList.Length + 2, j] = M4S[2, j];
            A[positionList.Length + 3, j + positionList.Length - 1] = M4S[2, j];
        }
        for (int i = 0, k = positionList.Length; i < k; i++)
        {
            B[0][i] = positionList[i].x;
            B[1][i] = positionList[i].y;
            B[2][i] = positionList[i].z;
        }
        B[0][positionList.Length] = HeadTailVelocity.V0.x;
        B[1][positionList.Length] = HeadTailVelocity.V0.y;
        B[2][positionList.Length] = HeadTailVelocity.V0.z;
        B[0][positionList.Length + 1] = HeadTailVelocity.V1.x;
        B[1][positionList.Length + 1] = HeadTailVelocity.V1.y;
        B[2][positionList.Length + 1] = HeadTailVelocity.V1.z;
        B[0][positionList.Length + 2] = HeadTailAcceleration.V0.x;
        B[1][positionList.Length + 2] = HeadTailAcceleration.V0.y;
        B[2][positionList.Length + 2] = HeadTailAcceleration.V0.z;
        B[0][positionList.Length + 3] = HeadTailAcceleration.V1.x;
        B[1][positionList.Length + 3] = HeadTailAcceleration.V1.y;
        B[2][positionList.Length + 3] = HeadTailAcceleration.V1.z;



        var constraints1 = CreateConstraints(A, B[0]);
        var constraints2 = CreateConstraints(A, B[1]);
        var constraints3 = CreateConstraints(A, B[2]);
        double[,] H = new double[positionList.Length + 4 - 1, positionList.Length + 4 - 1];

        for (int i = 0; i < positionList.Length + 3; i++)
            for (int j = 0; j < order; j++)
                for (int k = 0; k < order; k++)
                    if (i + j < positionList.Length + 3 && i + k < positionList.Length + 3)
                        H[i + j, i + k] += H4S[j, k];

        QuadraticObjectiveFunction func = new QuadraticObjectiveFunction(H, new double[positionList.Length + 4 - 1]);
        var solver = new GoldfarbIdnani(func, constraints1);
        solver.Minimize();
        controlPointsX = solver.Solution;
        solver = new GoldfarbIdnani(func, constraints2);
        solver.Minimize();
        controlPointsY = solver.Solution;
        solver = new GoldfarbIdnani(func, constraints3);
        solver.Minimize();
        controlPointsZ = solver.Solution;

        controlPointOptimizer.Optimize(this);

        ReallocTimeline();
    }

    public Vector3d Value(double timeInSecond)
    {
        int k = 2;
        if (timeline.Length < 5) return new();
        timeInSecond = Math.Max(0, Math.Min(timeInSecond, timeline[^5]));
        while (timeline[k + 1] < timeInSecond)
            k++;

        if (k + 3 >= timeline.Length)
        {
            k = k - 1;
        }

        var u = (timeInSecond - timeline[k]) / (timeline[k + 1] - timeline[k]);
        if (lastTimeIndex != k)
        {
            M4(k);
            lastTimeIndex = k;
        }
        var x = new double[] { 1, u, u * u, u * u * u }.Dot(M4S).Dot(controlPointsX.Get(k - 2, k + 2));
        var y = new double[] { 1, u, u * u, u * u * u }.Dot(M4S).Dot(controlPointsY.Get(k - 2, k + 2));
        var z = new double[] { 1, u, u * u, u * u * u }.Dot(M4S).Dot(controlPointsZ.Get(k - 2, k + 2));
        return new(x, y, z);


    }

    public IEnumerable<Vector3d> TrajectoryPoints(double fromWhen, double toWhen, double step)
    {
        List<Vector3d> data = [];
        for (var b = fromWhen + step; b <= toWhen + 1e-7; b += step)
            data.Add(Value(b));
        return data;
    }

    public override void Start()
    {
        H4S = new double[4, 4];
        var k = new double[1, 4] { { 0, 0, 0, 6 } }.Dot(M4S);
        H4S = H4S.Add(k.TransposeAndDot(k));

    }

    private void ReallocTimeline()
    {
        lastTimeIndex = -1;
        timeline = new double[controlPointsX.Length + order - 1];
        timeline[0] = -2 * _timeInterval;
        for (int i = 1; i < timeline.Length; i++)
            timeline[i] = timeline[i - 1] + _timeInterval;

        while (true)
        {
            var tmpControlPoints = new double[3, controlPointsX.Length];
            for (int j = 0; j < controlPointsX.Length - 1; j++)
            {
                tmpControlPoints[0, j] = 3 * (controlPointsX[j + 1] - controlPointsX[j]) / (timeline[j + 3] - timeline[j]);
                tmpControlPoints[1, j] = 3 * (controlPointsY[j + 1] - controlPointsY[j]) / (timeline[j + 3] - timeline[j]);
                tmpControlPoints[2, j] = 3 * (controlPointsZ[j + 1] - controlPointsZ[j]) / (timeline[j + 3] - timeline[j]);
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
            if (vMax < _vLimit)
                break;

            var i = index;

            var ratio = Math.Min(_ratioLimit, vMax / _vLimit) + 1e-4;
            var tOld = timeline[i + 2] - timeline[i - 2];
            var tNew = ratio * tOld;
            var tInt = (tNew - tOld) / 4;


            var head = i - 1;
            var tail = i + 2;

            for (var j = head; j <= tail; j++)
                timeline[j] = tInt * (j - head) + timeline[j];

            for (var j = tail + 1; j < timeline.Length; j++)
                timeline[j] = tInt * 4 + timeline[j];

        }

        while (true)
        {
            var tmpControlPoints = new double[3, controlPointsX.Length];
            for (int j = 0; j < controlPointsX.Length - 1; j++)
            {
                tmpControlPoints[0, j] = 3 * (controlPointsX[j + 1] - controlPointsX[j]) / (timeline[j + 3] - timeline[j]);
                tmpControlPoints[1, j] = 3 * (controlPointsY[j + 1] - controlPointsY[j]) / (timeline[j + 3] - timeline[j]);
                tmpControlPoints[2, j] = 3 * (controlPointsZ[j + 1] - controlPointsZ[j]) / (timeline[j + 3] - timeline[j]);
            }
            for (int j = 0; j < controlPointsX.Length - 2; j++)
            {
                tmpControlPoints[0, j] = 2 * (controlPointsX[j + 1] - controlPointsX[j]) / (timeline[j + 2] - timeline[j]);
                tmpControlPoints[1, j] = 2 * (controlPointsY[j + 1] - controlPointsY[j]) / (timeline[j + 2] - timeline[j]);
                tmpControlPoints[2, j] = 2 * (controlPointsZ[j + 1] - controlPointsZ[j]) / (timeline[j + 2] - timeline[j]);
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
            if (aMax < _aLimit)
                break;

            var i = index;

            var ratio = Math.Min(_ratioLimit, aMax / _aLimit) + 1e-4;
            var tOld = timeline[i + 2] - timeline[i - 2];
            var tNew = ratio * tOld;
            var tInt = (tNew - tOld) / 4;

            var head = i - 1;
            var tail = i + 2;

            for (var j = head; j <= tail; j++)
                timeline[j] = tInt * (j - head) + timeline[j];

            for (var j = tail + 1; j < timeline.Length; j++)
                timeline[j] = tInt * 4 + timeline[j];
        }
        var tTmp = timeline[2];
        for (int i = 0; i < timeline.Length; i++)
            timeline[i] -= tTmp;
    }


}