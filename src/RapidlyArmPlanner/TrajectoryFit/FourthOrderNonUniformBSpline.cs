using System.Diagnostics;
using Accord.Math.Optimization;

namespace RapidlyArmPlanner.TrajectoryFit;

class BSplineTrajectoryWithMinimalSnap
{
  double _value;
  public double Value => _value * 1e-4 + timeline[^1];
  private static readonly double[,] M4S = new double[4, 4]
  {
    { 1, 4, 1, 0 },
    { -3, 0, 3, 0 },
    { 3, -6, 3, 0 },
    { -1, 3, -3, 1 },
  }.Divide(6);
  double[] controlPoints;
  double[] timeline;

  private readonly double[,] M4Data = new double[4, 4];

  private double[,] M4(int i)
  {
    var tmp = Math.Pow(timeline[i + 1] - timeline[i], 2);

    M4Data[0, 0] = tmp / (timeline[i + 1] - timeline[i - 1]) / (timeline[i + 1] - timeline[i - 2]);
    M4Data[0, 2] =
      Math.Pow(timeline[i] - timeline[i - 1], 2)
      / (timeline[i + 2] - timeline[i - 1])
      / (timeline[i + 1] - timeline[i - 1]);
    M4Data[1, 2] =
      3
      * (timeline[i + 1] - timeline[i])
      * (timeline[i] - timeline[i - 1])
      / (timeline[i + 2] - timeline[i - 1])
      / (timeline[i + 1] - timeline[i - 1]);
    M4Data[2, 2] =
      3 * tmp / (timeline[i + 2] - timeline[i - 1]) / (timeline[i + 1] - timeline[i - 1]);
    M4Data[3, 3] = tmp / (timeline[i + 3] - timeline[i]) / (timeline[i + 2] - timeline[i]);
    M4Data[3, 2] =
      (-M4Data[2, 2] / 3)
      - M4Data[3, 3]
      - (tmp / (timeline[i + 2] - timeline[i]) / (timeline[i + 2] - timeline[i - 1]));

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

  public double MaxTime => timeline[^5];
  private const double _timeInterval = 0.1f;
  private const double _vLimit = 5;
  private const double _aLimit = 10;
  private const double _ratioLimit = 1.01;
  int lastIndex = -1;

  public static LinearConstraintCollection CreateConstraints(
    double[,] a,
    double[] b,
    LinkedList<(double value, double loose)> loose
  )
  {
    int length = a.GetLength(1);
    int length2 = b.Length;
    List<LinearConstraint> array = new();
    var head = loose.First;
    for (int i = 0; i < length2; i++)
    {
      if (i >= length2 - 2 || head.Value.loose == 0)
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
        constraint1.Value = b[i] - head.Value.loose;
        constraint2.Value = b[i] + head.Value.loose;
        array.Add(constraint1);
        array.Add(constraint2);
      }
      if (head.Next != null)
        head = head.Next;
    }

    return new LinearConstraintCollection(array);
  }

  public BSplineTrajectoryWithMinimalSnap(LinkedList<(double value, double loose)> values)
  {
    double[,] A = new double[values.Count + 2, values.Count + 4 - 1];
    double[] B = new double[values.Count + 2];

    for (int i = 0; i < values.Count; i++)
    for (int j = 0; j < 4; j++)
      A[i, i + j] = M4S[0, j];

    for (int j = 0; j < 4; j++)
    {
      A[values.Count, j] = M4S[1, j];
      A[values.Count + 1, j + values.Count - 1] = M4S[1, j];
    }
    var head = values.First;
    for (int i = 0, k = values.Count; i < k; i++)
    {
      B[i] = head.Value.value;
      head = head.Next;
    }

    double[,] H = new double[values.Count + 4 - 1, values.Count + 4 - 1];
    double[,] H4S = new double[4, 4];
    // for (double u = 0; u < 1; u += 0.1)
    // {
    //     var k = new double[1, 4] { { 0, 0, 2, 6 * u / 100 } }.Dot(M4S);
    //     H4S = H4S.Add(k.TransposeAndDot(k));
    // }
    var l = new double[1, 4]
    {
      { 0, 0, 0, 6 },
    }.Dot(M4S);
    H4S = H4S.Add(l.TransposeAndDot(l));

    for (int i = 0; i < values.Count + 3; i++)
    for (int j = 0; j < 4; j++)
    for (int k = 0; k < 4; k++)
      if (i + j < values.Count + 3 && i + k < values.Count + 3)
        H[i + j, i + k] += H4S[j, k];

    var constraints1 = CreateConstraints(A, B, values);

    QuadraticObjectiveFunction func = new QuadraticObjectiveFunction(
      H,
      new double[values.Count + 4 - 1]
    );
    var solver = new GoldfarbIdnani(func, constraints1);
    solver.Minimize();
    _value = solver.Value;
    controlPoints = solver.Solution;
    // var test = A.Dot(controlPoints);
    // lastIndex = -1;

    // ReAllocateTimeLine();
  }

  public static LinearConstraintCollection CreateConstraints(double[,] a, double[] b)
  {
    int length = a.GetLength(1);
    int length2 = b.Length;
    List<LinearConstraint> array = new();
    for (int i = 0; i < length2; i++)
    {
      if (i >= length2 - 3 || i == 0)
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
        constraint1.Value = b[i] - 1e-1;
        constraint2.Value = b[i] + 1e-1;
        array.Add(constraint1);
        array.Add(constraint2);
      }
    }

    return new LinearConstraintCollection(array);
  }

  public BSplineTrajectoryWithMinimalSnap(LinkedList<double> values)
  {
    double[,] A = new double[values.Count + 2, values.Count + 4 - 1];
    double[] B = new double[values.Count + 2];

    for (int i = 0; i < values.Count; i++)
    for (int j = 0; j < 4; j++)
      A[i, i + j] = M4S[0, j];

    for (int j = 0; j < 4; j++)
    {
      A[values.Count, j] = M4S[1, j];
      A[values.Count + 1, j + values.Count] = M4S[1, j];
    }
    var head = values.First;
    for (int i = 0, k = values.Count; i < k; i++)
    {
      B[i] = head.Value;
      head = head.Next;
    }

    double[,] H = new double[values.Count + 4 - 1, values.Count + 4 - 1];
    double[,] H4S = new double[4, 4];
    // for (double u = 0; u < 1; u += 0.1)
    // {
    //     var k = new double[1, 4] { { 0, 0, 2, 6 * u / 100 } }.Dot(M4S);
    //     H4S = H4S.Add(k.TransposeAndDot(k));
    // }
    var l = new double[1, 4]
    {
      { 0, 0, 0, 6 },
    }.Dot(M4S);
    H4S = H4S.Add(l.TransposeAndDot(l));

    for (int i = 0; i < values.Count + 3; i++)
    for (int j = 0; j < 4; j++)
    for (int k = 0; k < 4; k++)
      if (i + j < values.Count + 3 && i + k < values.Count + 3)
        H[i + j, i + k] += H4S[j, k];

    var constraints1 = CreateConstraints(A, B);

    QuadraticObjectiveFunction func = new QuadraticObjectiveFunction(
      H,
      new double[values.Count + 4 - 1]
    );
    var solver = new GoldfarbIdnani(func, constraints1);
    solver.Minimize();
    _value = solver.Value;
    controlPoints = solver.Solution;
    // var test = A.Dot(controlPoints);
    // lastIndex = -1;

    // ReAllocateTimeLine();
  }

  public double GetPosition(double time)
  {
    int k = 2;

    time = Math.Min(time, timeline[^5]);
    while (timeline[k + 1] < time)
      k++;

    if (k + 3 >= timeline.Length)
    {
      return 0;
    }

    if (lastIndex != k)
    {
      lastIndex = k;
      M4(k);
    }

    double u = (time - timeline[k]) / (timeline[k + 1] - timeline[k]);
    // var tmp2 = new double[,] { { 1, u, u * u, u * u * u } }.Dot(M4Data);
    // var tmp = new double[,] { { 1, u, u * u, u * u * u } }.Dot(M4Data).Dot(controlPoints.Get(k - 2, k + 2));
    // u = 0;
    return new double[] { 1, u, u * u, u * u * u }
      .Dot(M4Data)
      .Dot(controlPoints.Get(k - 2, k + 2));
  }

  public static void ReAllocTimeline(List<BSplineTrajectoryWithMinimalSnap> lines)
  {
    Debug.Assert(lines.Count > 0);

    var timeline = new double[lines[0].controlPoints.Length + 4 - 1];
    timeline[0] = -2 * _timeInterval;
    for (int i = 1; i < timeline.Length; i++)
      timeline[i] = timeline[i - 1] + _timeInterval;

    while (true)
    {
      var tmpControlPoints = new double[lines.Count, lines[0].controlPoints.Length];
      for (int k = 0; k < lines.Count; k++)
      for (int j = 0; j < lines[k].controlPoints.Length - 1; j++)
      {
        tmpControlPoints[k, j] =
          3
          * (lines[k].controlPoints[j + 1] - lines[k].controlPoints[j])
          / (timeline[j + 3] - timeline[j]);
      }

      double vMax = 0;
      int index = -1;

      for (int k = 2; k < timeline.Length - 3; k++)
      {
        var m_00 = (timeline[k + 1] - timeline[k]) / (timeline[k + 1] - timeline[k - 1]);
        var m_01 = (timeline[k] - timeline[k - 1]) / (timeline[k + 1] - timeline[k - 1]);

        var vs = tmpControlPoints
          .Get(0, 0, k - 2, k)
          .Dot(
            new double[,]
            {
              { m_00 },
              { m_01 },
            }
          );
        foreach (var v in vs)
          if (vMax < Math.Sqrt(v))
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
      var tmpControlPoints = new double[lines.Count, lines[0].controlPoints.Length];
      for (int k = 0; k < lines.Count; k++)
      for (int j = 0; j < lines[0].controlPoints.Length - 1; j++)
      {
        tmpControlPoints[k, j] =
          3
          * (lines[k].controlPoints[j + 1] - lines[k].controlPoints[j])
          / (timeline[j + 3] - timeline[j]);
      }
      for (int k = 0; k < lines.Count; k++)
      for (int j = 0; j < lines[0].controlPoints.Length - 2; j++)
      {
        tmpControlPoints[k, j] =
          2
          * (tmpControlPoints[k, j + 1] - tmpControlPoints[k, j])
          / (timeline[j + 2] - timeline[j]);
      }

      double aMax = 0;
      int index = -1;

      for (int k = 2; k < timeline.Length - 4; k++)
      {
        var a_s = tmpControlPoints.Get(0, 0, k - 2, k - 1);
        foreach (var a in a_s)
          if (aMax < Math.Abs(a))
          {
            index = k;
            aMax = Math.Abs(a);
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

    foreach (var line in lines)
      line.timeline = timeline;
  }
}
