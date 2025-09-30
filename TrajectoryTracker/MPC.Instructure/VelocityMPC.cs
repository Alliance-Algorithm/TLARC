using System.Numerics;
using Accord.Math;
using Accord.Math.Optimization;

namespace MPC.Instructure;

static class VelocityMPC<T> where T : ICarModel2D
{
    private const int ControlPredictLength = 40;
    private const int P = ControlPredictLength;
    private const int SizeX = 2;
    private const int SizeU = 2;
    private const double uMax = 2.0;
    private const double vMax = 6.0;
    private const double vMin = -vMax;
    private const double uMin = -uMax;

    private readonly static double Q = 20;
    private readonly static double R = 1;
    private readonly static double rho = 10;

    private readonly static double[] U = Accord.Math.Vector.Zeros(2);
    private readonly static LinearConstraintCollection constraintCollection = [];

    private readonly static double[,] A_ = new double[SizeX + SizeU, 2 * SizeX];

    public static Vector2 ControlVolume(Vector2 position, Vector2 velocity, T car, bool reset = false)
    {
        if (reset)
        {
            U[0] = velocity.X;
            U[1] = velocity.Y;
        }

        ref var x = ref position;
        var A = car.A;
        var B = car.B;
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

        var psi = Accord.Math.Vector.Zeros(SizeX + SizeU);
        for (int i = 0; i < SizeX; i++)
            psi[i] = x[i];
        for (int i = 0; i < SizeX; i++)
            psi[i + SizeX] = U[i];

        var E = W.Dot(psi).Subtract(refX);
        var H = Matrix.Zeros(P * SizeX + 1, P * SizeX + 1);
        var H_help = Z.Transpose().Dot(Z).Multiply(Q).AddToDiagonal(R);
        for (int i = 0; i < P * SizeX; i++)
            for (int j = 0; j < P * SizeX; j++)
                H[i, j] = H_help[i, j];

        H[P * SizeX, P * SizeX] = rho;
        H = H.Multiply(2);
        double[] G = [.. E.Dot(Z).Multiply(2).Multiply(Q), 0];

        constraintCollection.Clear();

        var ConstantA1 = new double[SizeX * P];
        var ConstantA2 = new double[SizeX * P];
        for (int i = 0; i < SizeX * P; i += 2)
        {
            ConstantA1[i] = 1;
            ConstantA2[i + 1] = 1;
            var c1 = ConstantA1.Copy();
            var c2 = ConstantA2.Copy();
            constraintCollection.Add(
                          new(SizeX * P)
                          {
                              CombinedAs = c1,
                              Value = vMin - U[0],
                              ShouldBe = ConstraintType.GreaterThanOrEqualTo,
                          }
                        );
            constraintCollection.Add(
              new(SizeX * P)
              {
                  CombinedAs = c1,
                  Value = vMax - U[0],
                  ShouldBe = ConstraintType.LesserThanOrEqualTo,
              }
            );
            constraintCollection.Add(
              new(SizeX * P)
              {
                  CombinedAs = c2,
                  Value = vMin - U[1],
                  ShouldBe = ConstraintType.GreaterThanOrEqualTo,
              }
            );
            constraintCollection.Add(
              new(SizeX * P)
              {
                  CombinedAs = c2,
                  Value = vMax - U[1],
                  ShouldBe = ConstraintType.LesserThanOrEqualTo,
              }
            );
        }
        for (int i = 0; i < SizeX * P; i++)
        {
            var ConstantB = new double[SizeX * P];
            ConstantB[i] = 1;
            constraintCollection.Add(
              new(SizeX * P)
              {
                  CombinedAs = ConstantB,
                  Value = uMin * car.ControlCycleTime,
                  ShouldBe = ConstraintType.GreaterThanOrEqualTo,
              }
            );
            constraintCollection.Add(
              new(SizeX * P)
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

        for (int i = 0; i < SizeX; i++)
            U[i] = solver.Solution[i] + U[i];

        return new System.Numerics.Vector2((float)U[0], (float)U[1]);
    }
}