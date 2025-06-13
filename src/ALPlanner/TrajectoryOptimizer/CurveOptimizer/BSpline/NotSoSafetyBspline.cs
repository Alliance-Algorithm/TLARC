using Accord;
using Accord.Math.Optimization;
using DecisionMaker.Information;
using TlarcKernel.TrajectoryOptimizer.Curves;

namespace ALPlanner.TrajectoryOptimizer.Curves;

class NotSoSafetyBSpline : Component, IKOrderBSpline
{

    Transform sentry;
    DecisionMakingInfo info;
    public Vector3d SentryPos;
    private double _vLimit = 2.0;
    private double _aLimit = 2.0;
    private double _ratioLimit = 1.01;
    public double[] controlPointsX = [];
    public double[] controlPointsY = [];
    public double[] timeline = [0];


    public double[][] ControlPoint => [controlPointsX, controlPointsY];

    public double MaxTime => timeline[^1];

    public DateTime _ConstructTime;
    public DateTime ConstructTime => _ConstructTime;


    public bool Check() => controlPointsX.Length != 0;
    double[,] tmpM = new double[4, 4] {
        {1  /6.0    ,4  /6.0    ,1  /6.0    ,0      },
        {-3 /6.0    ,0          ,3  /6.0    ,0      },
        {3  /6.0    ,-6 /6.0    ,3  /6.0    ,0      },
        {-1 /6.0    ,3  /6.0    ,-3 /6.0    ,1  /6.0},
    };
    double[,] tmpPosM = new double[2, 8] {
        {1  /6.0    ,4  /6.0    ,1  /6.0    ,0      ,0          ,0          ,0          ,0},
        {0          ,0          ,0          ,0      ,1  /6.0    ,4  /6.0    ,1  /6.0    ,0},
    };
    double[] tmpVelM = [ -3 / 6.0, 0,  3 / 6.0, 0];
    (double t_i_2, double t_i_1, double t_i, double t_i1, double t_i2, double t_i3) last_time = (0, 0, 0, 0, 0, 0);
    public double[,] M(double t_i_2, double t_i_1, double t_i, double t_i1, double t_i2, double t_i3)
    {
        if (last_time.t_i_2 == t_i_2 && last_time.t_i == t_i && last_time.t_i_1 == t_i_1 && last_time.t_i1 == t_i1 && last_time.t_i2 == t_i2 && last_time.t_i3 == t_i3) return tmpM;
        double m00 = Math.Pow(t_i1 - t_i, 2) / ((t_i1 - t_i_1) * (t_i1 - t_i_2));
        double m02 = Math.Pow(t_i - t_i_1, 2) / ((t_i2 - t_i_1) * (t_i1 - t_i_1));
        double m12 = 3 * (t_i1 - t_i) * (t_i - t_i_1) / ((t_i2 - t_i_1) * (t_i1 - t_i_1));
        double m22 = 3 * Math.Pow(t_i1 - t_i, 2) / ((t_i2 - t_i_1) * (t_i1 - t_i_1));
        double m33 = Math.Pow(t_i1 - t_i, 2) / ((t_i3 - t_i) * (t_i2 - t_i));
        double m32 = -m22 / 3 - m33 - Math.Pow(t_i1 - t_i, 2) / ((t_i2 - t_i) * (t_i2 - t_i_1));

        tmpM = new double[4, 4] {
            {m00        ,1 - m00 - m02      ,m02    ,0  },
            {-3 * m00   ,3 * m00 - m12      ,m12    ,0  },
            {3  * m00   ,-3 * m00 - m22     ,m22    ,0  },
            {-m00       ,m00 - m32 - m33    ,m32    ,m33},
        };
        last_time.t_i = t_i;
        last_time.t_i1 = t_i1;
        last_time.t_i2 = t_i2;
        last_time.t_i3 = t_i3;
        last_time.t_i_1 = t_i_1;
        last_time.t_i_2 = t_i_2;
        return tmpM;
    }
    private static double[,] H4S = new double[4, 4];

    public override void Start()
    {
        H4S = new double[4, 4];
        var k = new double[1, 4] { { 0, 0, 0, 6 } }.Dot(tmpM);
        H4S = H4S.Add(k.TransposeAndDot(k));
    }
    public override void Update()
    {
        SentryPos = sentry.Position;
        _aLimit = Math.Sqrt((info.PowerLimit - 10) / 22.0) * 0.8;
        _vLimit = _aLimit * 2;
        // TlarcSystem.LogInfo($"{_aLimit}");
    }
    private void Reset()
    {
        controlPointsX = [];
        controlPointsY = [];
        timeline = [0];
    }
    public void Construction(ConstraintCollection positionList)
    {
        if (positionList.Length == 0)
            return;
        var time = positionList.TimeStep;
        _ConstructTime = DateTime.Now;
        LinearConstraintCollection linearConstraint = [];
        LinearConstraintCollection linearConstraintLoose = [];

        var controlPointsLength = positionList.Length + 4;
        var tmpX = positionList.XBegin;
        var tmpY = positionList.YBegin;
        for (int i = 0; i < positionList.Length; i++)
        {
            if (i == 0 || i == positionList.Length - 1)
            {
                double[,] A = tmpX.Rotation.Dot(tmpPosM);  //Ax = B
                if (i == positionList.Length - 1) i++;
                foreach (var c in ConstraintHelper.BuildConstraint(controlPointsLength * 2, A.GetRow(0), tmpX, variablesAtIndices:
                [i                      , i + 1                         , i + 2                        , i + 3                      ,
                i + controlPointsLength , i + controlPointsLength + 1   , i + controlPointsLength + 2  , i + controlPointsLength + 3]))
                {
                    linearConstraint.Add(c);
                    linearConstraintLoose.Add(c);
                }
                foreach (var c in ConstraintHelper.BuildConstraint(controlPointsLength * 2, A.GetRow(1), tmpY, variablesAtIndices:
                [i                      , i + 1                         , i + 2                        , i + 3                      ,
                i + controlPointsLength , i + controlPointsLength + 1   , i + controlPointsLength + 2  , i + controlPointsLength + 3]))
                {
                    linearConstraint.Add(c);
                    linearConstraintLoose.Add(c);
                }
            }
            if (i > 0 && i < positionList.Length - 1 )
            {
                foreach (var c in ConstraintHelper.BuildConstraint(controlPointsLength * 2, tmpX.Rotation.GetRow(0), tmpX, variablesAtIndices: [i + 1, i + controlPointsLength + 1]))
                {
                    linearConstraint.Add(c);
                    linearConstraintLoose.Add(c);
                }
                foreach (var c in ConstraintHelper.BuildConstraint(controlPointsLength * 2, tmpX.Rotation.GetRow(1), tmpY, variablesAtIndices: [i + 1, i + controlPointsLength + 1]))
                {
                    linearConstraint.Add(c);
                    linearConstraintLoose.Add(c);
                }
            }
            if (i > 0&& i < positionList.Length - 1 )
            {
                foreach (var c in ConstraintHelper.BuildConstraint(controlPointsLength * 2, tmpX.Rotation.GetRow(0), tmpX, variablesAtIndices: [i + 2, i + controlPointsLength + 2]))
                    linearConstraint.Add(c);
                foreach (var c in ConstraintHelper.BuildConstraint(controlPointsLength * 2, tmpX.Rotation.GetRow(1), tmpY, variablesAtIndices: [i + 2, i + controlPointsLength + 2]))
                    linearConstraint.Add(c);
            }
            tmpX = tmpX.next;
            tmpY = tmpY.next;
        }

        linearConstraint.Add(ConstraintHelper.CreateLinearConstraint
                (
                    controlPointsLength * 2,
                    tmpVelM.Divide(time),
                    ConstraintType.EqualTo,
                    [0, 1, 2, 3],
                    sentry.Velocity.x
                ));
        linearConstraint.Add(ConstraintHelper.CreateLinearConstraint
        (
            controlPointsLength * 2,
            tmpVelM.Divide(time),
            ConstraintType.EqualTo,
            [0 + controlPointsLength, 1 + controlPointsLength, 2 + controlPointsLength, 3 + controlPointsLength],
            sentry.Velocity.y
        ));
        linearConstraint.Add(ConstraintHelper.CreateLinearConstraint
        (
            controlPointsLength * 2,
            tmpVelM.Divide(time),
            ConstraintType.EqualTo,
            [-4 + controlPointsLength * 2, -3 + controlPointsLength * 2, -2 + controlPointsLength * 2, -1 + controlPointsLength * 2],
            0
        ));
        linearConstraint.Add(ConstraintHelper.CreateLinearConstraint
        (
            controlPointsLength * 2,
            tmpVelM.Divide(time),
            ConstraintType.EqualTo,
            [-4 + controlPointsLength, -3 + controlPointsLength, -2 + controlPointsLength, -1 + controlPointsLength],
            0
        ));
        double[,] H = new double[controlPointsLength * 2, controlPointsLength * 2];

        for (int i = 0; i < controlPointsLength - 3; i++)
            for (int j = 0; j < 4; j++)
                for (int k = 0; k < 4; k++)
                    if (i + j < controlPointsLength && i + k < controlPointsLength)
                        H[i + j, i + k] += H4S[j, k];
        for (int i = controlPointsLength - 3 ; i < controlPointsLength; i++)
                    H[i , i ] += 1e-4;
        for (int i = controlPointsLength; i < controlPointsLength * 2 - 3; i++)
            for (int j = 0; j < 4; j++)
                for (int k = 0; k < 4; k++)
                    if (i + j < controlPointsLength * 2  && i + k < controlPointsLength * 2  )
                        H[i + j, i + k] += H4S[j, k];
        for (int i = controlPointsLength * 2 - 3; i < controlPointsLength * 2; i++)
                    H[i , i ] += 1e-4;
                    
        QuadraticObjectiveFunction func = new QuadraticObjectiveFunction(H, new double[controlPointsLength * 2]);
        var solver = new GoldfarbIdnani(func, linearConstraint);
        solver.Minimize();
        controlPointsX = solver.Solution[..controlPointsLength];
        controlPointsY = solver.Solution[controlPointsLength..];

        timeline = new double[controlPointsLength + 3];
        timeline[0] = -2 * time;
        timeline[1] = -1 * time;
        for (int i = 0; i < controlPointsLength; i++)
            timeline[i + 3] = timeline[i + 2] + time;
        if (solver.Status == GoldfarbIdnaniStatus.Success)
        {
            ReallocTimeline();
        }
        else
        {
            TlarcSystem.LogWarning($"Failed to build safety B-spline:{solver.Status.ToString()} try loose version");
            var solver_loose = new GoldfarbIdnani(func, linearConstraintLoose);
            solver_loose.Minimize();
            controlPointsX = solver_loose.Solution[..controlPointsLength];
            controlPointsY = solver_loose.Solution[controlPointsLength..];

            timeline = new double[controlPointsLength + 3];
            timeline[0] = -2 * time;
            timeline[1] = -1 * time;
            for (int i = 0; i < controlPointsLength; i++)
                timeline[i + 3] = timeline[i + 2] + time;
            if (solver_loose.Status == GoldfarbIdnaniStatus.Success)
            {
                ReallocTimeline();
            }
            else
            {
                TlarcSystem.LogWarning($"Finally failed to build safety B-spline:{solver.Status.ToString()}");
                Reset();
            }
        }
    }

    public void OptimizeTrajectory()
    {
        throw new NotImplementedException();
    }

    private int lastTimeIndex = -1;
    public Vector3d Value(double timeInSecond)
    {
        int k = 2;
        if (timeline.Length < 5) return SentryPos;
        timeInSecond = Math.Max(0, Math.Min(timeInSecond, timeline[^5]));
        while (timeline[k + 1] < timeInSecond)
            k++;

        if (k + 3 >= timeline.Length)
        {
            k--;
        }

        var u = (timeInSecond - timeline[k]) / (timeline[k + 1] - timeline[k]);
        if (lastTimeIndex != k)
        {
            M(timeline[k - 2], timeline[k - 1], timeline[k], timeline[k + 1], timeline[k + 2], timeline[k + 3]);
            lastTimeIndex = k;
        }
        var x = new double[] { 1, u, u * u, u * u * u }.Dot(tmpM).Dot(controlPointsX.Get(k - 2, k + 2));
        var y = new double[] { 1, u, u * u, u * u * u }.Dot(tmpM).Dot(controlPointsY.Get(k - 2, k + 2));
        return new(x, y, 0);
    }
    public IEnumerable<Vector3d> TrajectoryPoints(double fromWhen, double toWhen, double step)
    {
        List<Vector3d> data = [];
        if (toWhen <= fromWhen || step < 0.01)
            return data;
        for (var b = fromWhen + step; b <= toWhen + 1e-3; b += step)
            data.Add(Value(b));
        return data;
    }
    public IEnumerable<Vector3d> AcceleratePoints(double fromWhen, double toWhen, double step)
    {
        List<Vector3d> data = [];
        List<Vector3d> tmpData = [];
        for (var b = fromWhen + step; b <= toWhen + 1e-3; b += step)
            data.Add(Value(b));
        for (int i = 1; i < data.Count; i++)
            tmpData.Add((data[i] - data[i - 1]) / step);
        data = [];
        for (int i = 1; i < tmpData.Count; i++)
            data.Add((tmpData[i] - tmpData[i - 1]) / step);
        return data;
    }
    public Vector3d Velocity(double timeInSecond)
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
            M(timeline[k - 2], timeline[k - 1], timeline[k], timeline[k + 1], timeline[k + 2], timeline[k + 3]);
            lastTimeIndex = k;
        }
        var delta = timeline[k + 1] - timeline[k];
        var x = new double[] { 0, 1, 2 * u, 3 * u * u }.Dot(tmpM).Dot(controlPointsX.Get(k - 2, k + 2)) / delta;
        var y = new double[] { 0, 1, 2 * u, 3 * u * u }.Dot(tmpM).Dot(controlPointsY.Get(k - 2, k + 2)) / delta;
        return new(x, y, 0);
    }
    public IEnumerable<Vector3d> VelocitiesPoints(double fromWhen, double toWhen, double step)
    {
        List<Vector3d> data = [];
        List<Vector3d> tmpData = [];
        for (var b = fromWhen + step; b <= toWhen + 1e-3; b += step)
            data.Add(Value(b));
        for (int i = 1; i < data.Count; i++)
            tmpData.Add((data[i] - data[i - 1]) / step);
        return tmpData;
    }
    private void ReallocTimeline()
    {
        if (timeline.Length == 3)
            return;
            
        while (true)
        {

            double aMax = 0;
            int index = -1;

            for (int k = 2; k < controlPointsX.Length - 1; k++)
            {

                var delta = timeline[k + 1] - timeline[k];
                M(timeline[k - 2], timeline[k - 1], timeline[k], timeline[k + 1], timeline[k + 2], timeline[k + 3]);
                var x = new double[] { 0, 0, 2, 0 }.Dot(tmpM).Dot(controlPointsX.Get(k - 2, k + 2)) / delta / delta;
                var y = new double[] { 0, 0, 2, 0 }.Dot(tmpM).Dot(controlPointsY.Get(k - 2, k + 2)) / delta / delta;
                var a = Math.Sqrt(x * x + y * y);
                if (aMax < a)
                {
                    index = k;
                    aMax = a;
                }
            }
            if (aMax < _aLimit){
                TlarcSystem.LogInfo($"aMax={aMax},_aLimit={_aLimit}");
                break;
            }

            var i = index;

            var ratio = Math.Min(_ratioLimit, aMax / _aLimit) + 1e-4;
            var tOld = timeline[i + 2] - timeline[i - 2];
            var tNew = ratio * tOld;
            var tInt = (tNew - tOld) / 4;

            var head = i - 1;
            var tail = i + 2;

            for (var j = head; j <= tail; j++)
                timeline[j] = tInt * (j - head + 1) + timeline[j];

            for (var j = tail + 1; j < timeline.Length; j++)
                timeline[j] = tInt * 4 + timeline[j];
        }
        while (true)
        {
            double vMax = 0;
            int index = -1;

            for (int k = 2; k < controlPointsX.Length - 1; k++)
            {
                var delta = timeline[k + 1] - timeline[k];
                M(timeline[k - 2], timeline[k - 1], timeline[k], timeline[k + 1], timeline[k + 2], timeline[k + 3]);
                var x = new double[] { 0, 1, 0, 0 }.Dot(tmpM).Dot(controlPointsX.Get(k - 2, k + 2)) / delta;
                var y = new double[] { 0, 1, 0, 0 }.Dot(tmpM).Dot(controlPointsY.Get(k - 2, k + 2)) / delta;
                var v = Math.Sqrt(x * x + y * y);
                if (vMax < v)
                {
                    index = k;
                    vMax = v;
                }
            }
            if (vMax < _vLimit)
            {
                TlarcSystem.LogInfo($"vMax={vMax},_vLimit={_vLimit}");
                break;
            }

            var i = index;

            var ratio = Math.Min(_ratioLimit, vMax / _vLimit) + 1e-4;
            var tOld = timeline[i + 2] - timeline[i - 2];
            var tNew = ratio * tOld;
            var tInt = (tNew - tOld) / 4;


            var head = i - 1;
            var tail = i + 2;

            for (var j = head; j <= tail; j++)
                timeline[j] = tInt * (j - head + 1) + timeline[j];

            for (var j = tail + 1; j < timeline.Length; j++)
                timeline[j] = tInt * 4 + timeline[j];

        }

        var tTmp = timeline[2];
        for (int i = 0; i < timeline.Length; i++)
            timeline[i] -= tTmp;
        TlarcSystem.LogInfo($"Total Time：{MaxTime}s");
    }

    public void Construction(Vector3d point)
    {
        controlPointsX = [point.x, point.x, point.x, point.x];
        controlPointsY = [point.y, point.y, point.y, point.y];
        timeline = [-2, -1, 0, 1, 2, 3, 4];
    }
}