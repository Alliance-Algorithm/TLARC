using Accord.Math.Optimization;
using ALPlanner.TrajectoryOptimizer;
using TlarcKernel.TrajectoryOptimizer.Curves;

namespace Maps;

class SafetyBSpline : Component, IKOrderBSpline
{

    Transform sentry;

    public double[] controlPointsX = [];
    public double[] controlPointsY = [];
    public double[] controlPointsZ = [];
    public double[] timeline = [];


    public double[][] ControlPoint => throw new NotImplementedException();

    public double MaxTime => throw new NotImplementedException();

    public DateTime ConstructTime => throw new NotImplementedException();


    public bool Check() => controlPointsX.Length != 0;
    double[,] tmpM = new double[4, 4] {
        {1  /6.0    ,4  /6.0    ,1  /6.0    ,0},
        {-3 /6.0    ,0          ,3  /6.0    ,0},
        {3  /6.0    ,-6 /6.0    ,3  /6.0    ,0},
        {-1 /6.0    ,3  /6.0    ,-3 /6.0    ,1},
    };
    double[,] tmpPosM = new double[2, 8] {
        {1  /6.0    ,4  /6.0    ,1  /6.0    ,0      ,0          ,0          ,0          ,0},
        {0          ,0          ,0          ,0      ,1  /6.0    ,4  /6.0    ,1  /6.0    ,0},
    };
    double[] tmpVelM = [2 * -3 / 6.0, 0, 2 * 3 / 6.0, 0];
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
    public void Construction(ConstraintCollection positionList)
    {
        var time = positionList.TimeStep;

        LinearConstraintCollection linearConstraint = [];

        var controlPointsLength = positionList.Length + 3;
        var tmpX = positionList.XBegin;
        var tmpY = positionList.YBegin;
        for (int i = 0; i < positionList.Length; i++)
        {
            double[,] A = tmpX.Rotation.Dot(tmpPosM);  //Ax = B
            foreach (var c in ConstraintHelper.BuildConstraint(A.GetRow(0), tmpX, variablesAtIndices:
            [i                      , i + 1                         , i + 2                        , i + 3                      ,
            i + controlPointsLength , i + controlPointsLength + 1   , i + controlPointsLength + 2  , i + controlPointsLength + 3]))
                linearConstraint.Add(c);
            foreach (var c in ConstraintHelper.BuildConstraint(A.GetRow(1), tmpY, variablesAtIndices:
            [i                      , i + 1                         , i + 2                        , i + 3                      ,
            i + controlPointsLength , i + controlPointsLength + 1   , i + controlPointsLength + 2  , i + controlPointsLength + 3]))
                linearConstraint.Add(c);

            foreach (var c in ConstraintHelper.BuildConstraint([1], tmpX, variablesAtIndices: [i]))
                linearConstraint.Add(c);
            foreach (var c in ConstraintHelper.BuildConstraint([1], tmpY, variablesAtIndices: [i + controlPointsLength]))
                linearConstraint.Add(c);
        }
        linearConstraint.Add(new()
        {
            CombinedAs = tmpVelM.Divide(time),
            ShouldBe = ConstraintType.EqualTo,
            VariablesAtIndices = [0, 1, 2, 3],
            Value = sentry.Velocity.x
        });
        linearConstraint.Add(new()
        {
            CombinedAs = tmpVelM.Divide(time),
            ShouldBe = ConstraintType.EqualTo,
            VariablesAtIndices = [0 + controlPointsLength, 1 + controlPointsLength, 2 + controlPointsLength, 3 + controlPointsLength],
            Value = sentry.Velocity.y
        });
        double[,] H = new double[controlPointsLength * 2, controlPointsLength * 2];

        for (int i = 0; i < positionList.Length; i++)
            for (int j = 0; j < 4; j++)
                for (int k = 0; k < 4; k++)
                    if (i + j < positionList.Length + 3 && i + k < positionList.Length + 3)
                        H[i + j, i + k] += H4S[j, k];
        for (int i = positionList.Length; i < positionList.Length + 3; i++)
            for (int j = 0; j < 4; j++)
                for (int k = 0; k < 4; k++)
                    if (i + j < positionList.Length + 3 && i + k < positionList.Length + 3)
                        H[i + j, i + k] += 1e-4;
        for (int i = controlPointsLength; i < controlPointsLength + positionList.Length; i++)
            for (int j = 0; j < 4; j++)
                for (int k = 0; k < 4; k++)
                    if (i + j < positionList.Length + 3 && i + k < positionList.Length + 3)
                        H[i + j, i + k] += H4S[j, k];
        for (int i = controlPointsLength + positionList.Length; i < controlPointsLength + positionList.Length + 3; i++)
            for (int j = 0; j < 4; j++)
                for (int k = 0; k < 4; k++)
                    if (i + j < positionList.Length + 3 && i + k < positionList.Length + 3)
                        H[i + j, i + k] += 1e-4;



    }

    public void OptimizeTrajectory()
    {
        throw new NotImplementedException();
    }

    private int lastTimeIndex = -1;
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
            M(timeline[k - 2], timeline[k - 1], timeline[k], timeline[k + 1], timeline[k + 2], timeline[k + 3]);
            lastTimeIndex = k;
        }
        var x = new double[] { 1, u, u * u, u * u * u }.Dot(tmpM).Dot(controlPointsX.Get(k - 2, k + 2));
        var y = new double[] { 1, u, u * u, u * u * u }.Dot(tmpM).Dot(controlPointsY.Get(k - 2, k + 2));
        var z = new double[] { 1, u, u * u, u * u * u }.Dot(tmpM).Dot(controlPointsZ.Get(k - 2, k + 2));
        return new(x, y, z);
    }
    public IEnumerable<Vector3d> TrajectoryPoints(double fromWhen, double toWhen, double step)
    {
        List<Vector3d> data = [];
        for (var b = fromWhen + step; b <= toWhen + 1e-3; b += step)
            data.Add(Value(b));
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
        var z = new double[] { 0, 1, 2 * u, 3 * u * u }.Dot(tmpM).Dot(controlPointsZ.Get(k - 2, k + 2)) / delta;
        return new(x, y, z);
    }
    public IEnumerable<Vector3d> VelocitiesPoints(double fromWhen, double toWhen, double step)
    {
        List<Vector3d> data = [];
        for (var b = fromWhen + step; b <= toWhen + 1e-3; b += step)
            data.Add(Velocity(b));
        return data;
    }
}