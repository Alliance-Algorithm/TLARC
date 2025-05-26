using Accord.Math.Optimization;
using TlarcKernel;

namespace TrajectoryTracer.Controller;

class VelocityMPC : Component, IPositionVelocityController
{
  [ComponentReferenceFiled]
  ICarModel car;
  Transform sentry;
  private int ControlPredictLength = 40;
  public int P => ControlPredictLength;
  public double Q = 20;
  public double R = 1;
  public double rho = 10;
  public double[] U;
  public double[] uOut;
  public const double uMax = 2.1;
  public const double uMin = -uMax;
  public const double vMax = 2.1;
  public const double vMin = -vMax;

  LinearConstraintCollection constraintCollection;

  public Vector3d ControlVolume(Vector3d Position)
  {
    U ??= Vector.Zeros(car.ControlVolumeSize);
    uOut ??= Vector.Zeros(car.ControlVolumeSize);
    var x = car.X;
    var A = car.A;
    var B = car.B;

    var A_ = new double[A.GetLength(0) + B.GetLength(0), 2 * B.GetLength(1)];
    for (int i = 0; i < A.GetLength(0); i++)
      for (int j = 0; j < A.GetLength(1); j++)
        A_[i, j] = A[i, j];
    for (int i = 0; i < B.GetLength(0); i++)
      for (int j = 0; j < B.GetLength(1); j++)
        A_[i, j + A.GetLength(1)] = B[i, j];
    for (int i = 0; i < B.GetLength(0); i++)
      A_[i + A.GetLength(0), i + A.GetLength(1)] = 1;

    var B_ = new double[B.GetLength(1) + B.GetLength(0), B.GetLength(1)];
    for (int i = 0; i < B.GetLength(0); i++)
      for (int j = 0; j < B.GetLength(1); j++)
        B_[i, j] = B[i, j];
    for (int i = 0; i < B.GetLength(1); i++)
      B_[i + B.GetLength(0), i] = 1;

    var tmpB = Matrix.Zeros(A.GetLength(0), A_.GetLength(0));
    for (int i = 0; i < tmpB.GetLength(0); i++)
      tmpB[i, i] = 1;

    var tmpA = tmpB.Dot(A_);
    var refX = car.RefX(ControlPredictLength);
    var Z = Matrix.Zeros(A.GetLength(0) * P, B_.GetLength(1) * P);
    var W = Matrix.Zeros(A.GetLength(0) * P, A_.GetLength(1));

    for (int i = 0; i < P; i++)
    {
      tmpB = tmpB.Dot(B_);
      for (int j = 0; j < tmpA.GetLength(0); j++)
        for (int k = 0; k < tmpA.GetLength(1); k++)
          W[tmpA.GetLength(0) * i + j, k] = tmpA[j, k];

      for (int j = 0; j < tmpB.GetLength(0); j++)
        for (int k = 0; k < tmpB.GetLength(1); k++)
          Z[tmpB.GetLength(0) * i + j, k] = tmpB[j, k];
      for (int j = 0; j < tmpB.GetLength(0); j++)
        for (int k = 0; k < tmpB.GetLength(1) * (i - 1); k++)
          Z[tmpB.GetLength(0) * i + j, k + tmpB.GetLength(1)] = Z[tmpB.GetLength(0) * (i - 1) + j, k];

      tmpB = tmpA.Copy();
      tmpA = tmpA.Dot(A_);
    }

    var psi = Vector.Zeros(x.Length + car.SizeU);
    // U[0] = sentry.Velocity.x;
    // U[1] = sentry.Velocity.y;
    for (int i = 0; i < x.Length; i++)
      psi[i] = x[i];
    for (int i = 0; i < car.ControlVolumeSize; i++)
      psi[i + x.Length] = U[i];

    var E = W.Dot(psi).Subtract(refX);
    var H = Matrix.Zeros(P * car.ControlVolumeSize + 1, P * car.ControlVolumeSize + 1);
    var H_help = Z.Transpose().Dot(Z).Multiply(Q).AddToDiagonal(R);
    for (int i = 0; i < P * car.ControlVolumeSize; i++)
      for (int j = 0; j < P * car.ControlVolumeSize; j++)
        H[i, j] = H_help[i, j];

    H[P * car.ControlVolumeSize, P * car.ControlVolumeSize] = rho;
    H = H.Multiply(2);
    double[] G = [.. E.Dot(Z).Multiply(2).Multiply(Q), 0];

    constraintCollection = new();

    var ConstantA1 = new double[car.ControlVolumeSize * P];
    var ConstantA2 = new double[car.ControlVolumeSize * P];
    for (int i = 0; i < car.ControlVolumeSize * P; i += 2)
    {
      ConstantA1[i] = 1;
      ConstantA2[i + 1] = 1;
      var c1 = ConstantA1.Copy();
      var c2 = ConstantA1.Copy();
      constraintCollection.Add(
        new(car.ControlVolumeSize * P)
        {
          CombinedAs = c1,
          Value = vMin - U[0],
          ShouldBe = ConstraintType.GreaterThanOrEqualTo,
        }
      );
      constraintCollection.Add(
        new(car.ControlVolumeSize * P)
        {
          CombinedAs = c1,
          Value = vMax - U[0],
          ShouldBe = ConstraintType.LesserThanOrEqualTo,
        }
      );
      constraintCollection.Add(
        new(car.ControlVolumeSize * P)
        {
          CombinedAs = c2,
          Value = vMin - U[1],
          ShouldBe = ConstraintType.GreaterThanOrEqualTo,
        }
      );
      constraintCollection.Add(
        new(car.ControlVolumeSize * P)
        {
          CombinedAs = c2,
          Value = vMax - U[1],
          ShouldBe = ConstraintType.LesserThanOrEqualTo,
        }
      );
    }
    for (int i = 0; i < car.ControlVolumeSize * P; i++)
    {
      var ConstantB = new double[car.ControlVolumeSize * P];
      ConstantB[i] = 1;
      constraintCollection.Add(
        new(car.ControlVolumeSize * P)
        {
          CombinedAs = ConstantB,
          Value = uMin * car.ControlCycleTime,
          ShouldBe = ConstraintType.GreaterThanOrEqualTo,
        }
      );
      constraintCollection.Add(
        new(car.ControlVolumeSize * P)
        {
          CombinedAs = ConstantB,
          Value = uMax * car.ControlCycleTime,
          ShouldBe = ConstraintType.LesserThanOrEqualTo,
        }
      );
    }

    var quadratic = new QuadraticObjectiveFunction(H, G);

    GoldfarbIdnani solver = new(quadratic, constraintCollection);
    solver.Minimize();
    uOut = solver.Solution[0..car.ControlVolumeSize];
    U = U.Add(uOut);
    if (car.ControlVolumeSize == 2)
      return new Vector3d(U[0], U[1], 0);
    else
      return new Vector3d(U[0], U[1], U[2]);
  }
}
