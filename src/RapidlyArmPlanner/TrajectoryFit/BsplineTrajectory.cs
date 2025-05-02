namespace RapidlyArmPlanner.TrajectoryFit;

class BSplineTrajectory
{
  private static readonly double[,] M4S = new double[4, 4]
  {
    { 1, 4, 1, 0 },
    { -3, 0, 3, 0 },
    { 3, -6, 3, 0 },
    { -1, 3, -3, 1 },
  }.Divide(6);
  readonly double[] controlPoints;
  double[] timeline;

  public double MaxTime => timeline[^3];
  private readonly double _timeInterval = 0.1f;
  private readonly double _vLimit = 3;

  // private readonly double _aLimit = 3;
  // private readonly double _ratioLimit = 1.01;
  int lastIndex = -1;

  public BSplineTrajectory(double[] values)
  {
    double[,] A = new double[values.Length + 2, values.Length + 2];
    for (int i = 0; i < values.Length; i++)
    {
      for (int j = 0; j < 3; j++)
      {
        A[i, i + j] = M4S[0, j];
      }
    }
    for (int j = 0; j < 3; j++)
    {
      A[values.Length, j] = M4S[1, j];
      A[values.Length + 1, values.Length + j - 1] = M4S[1, j];
    }

    double[] b = new double[values.Length + 2];
    for (int i = 0; i < values.Length; i++)
    {
      b[i] = values[i];
    }
    b[values.Length] = 0;
    b[values.Length + 1] = 0;

    controlPoints = A.Solve(b);

    ReAllocateTimeLine();
  }

  public double GetPosition(double time)
  {
    int k = 2;

    time = Math.Min(time, timeline[^4]);
    while (timeline[k + 1] < time)
      k++;

    if (k + 2 > controlPoints.Length)
    {
      k = controlPoints.Length - 2;
      time = timeline[k + 1];
    }

    if (lastIndex != k)
    {
      lastIndex = k;
    }

    double u = (time - timeline[k]) / (timeline[k + 1] - timeline[k]);

    return new double[] { 1, u, u * u, u * u * u }
      .Dot(M4S)
      .Dot(controlPoints.Get(k - 2, k + 2));
  }

  public double ReAllocateTimeLine(double ratio = 0)
  {
    if (ratio != 0)
    {
      for (int i = 0; i < timeline.Length; i++)
        timeline[i] *= ratio;
      return ratio;
    }

    timeline = new double[controlPoints.Length + 4 - 1];
    timeline[0] = -2 * _timeInterval;
    for (int i = 1; i < timeline.Length; i++)
      timeline[i] = timeline[i - 1] + _timeInterval;

    bool velfea = false;
    while (true)
    {
      if (!velfea)
      {
        var tmpControlPoints = new double[controlPoints.Length];
        for (int j = 0; j < controlPoints.Length - 1; j++)
        {
          tmpControlPoints[j] =
            3 * (controlPoints[j + 1] - controlPoints[j]) / (timeline[j + 3] - timeline[j]);
        }
        double vMax = 0;

        for (int k = 2; k < timeline.Length - 3; k++)
        {
          var m_00 = (timeline[k + 1] - timeline[k]) / (timeline[k + 1] - timeline[k - 1]);
          var m_01 = (timeline[k] - timeline[k - 1]) / (timeline[k + 1] - timeline[k - 1]);

          var v = tmpControlPoints.Get(k - 2, k - 1).Dot(new double[] { m_00, m_01 });
          if (vMax < v)
            vMax = v;
        }
        if (vMax < _vLimit)
          break;

        if (ratio == 0)
          return vMax / _vLimit + 1e-4;
      }
    }
    return 0;
  }
}
