using Accord.Math.Optimization;
using TlarcKernel;
namespace TrajectoryTracer.Controller;

class VelocityMPC : Component, IPositionVelocityController
{
    ICarModel car;
    private int ControlPredictLength = 40;
    public int P => ControlPredictLength;
    public double[,]? Q = null;
    public double[,]? W = null;
    public double[,]? ConstantA = null;
    public double[]? ConstantB = null;
    public const double uMax = 10;
    public const double uMin = -uMax;
    public Vector3d ControlVolume(Vector3d Position)
    {
        var x = car.X.Transpose();
        var A = car.A;
        var B = car.B;
        var tmpA = A.Copy();
        var rowACount = A.GetLength(0);
        var colBCount = B.GetLength(1);
        double[,] refXExt = new double[rowACount * P, 1];
        var refX = car.RefX;
        var psi = Matrix.Zeros(rowACount * P, rowACount);
        var theta = Matrix.Zeros(rowACount * P, colBCount * P);
        var tmpC = B.Copy();

        for (int i = 0; i < P; i++)
        {
            for (int j = 0; j < rowACount; j++)
            {
                for (int k = 0; k < rowACount; k++)
                    psi[rowACount * i + j, k] = tmpA[j, k];
                refXExt[rowACount * i + j, 1] = refX[j];
            }
            for (int j = 0; j < rowACount; j++)
                for (int k = 0; k < colBCount; k++)
                    theta[rowACount * i + j, k] = tmpC[j, k];
            for (int j = 0; j < rowACount; j++)
                for (int k = 0; k < colBCount * (i - 1); k++)
                    theta[rowACount * i + j, k + colBCount] = theta[rowACount * (i - 1) + j, k];
            tmpA = tmpA.Dot(A);
            tmpC = tmpC.Dot(A);
        }


        var E = psi.Dot(x).Subtract(refXExt);
        var H =
                    theta.
                TransposeAndDot(
                    Q ??= Matrix.Identity(rowACount * P).Multiply(10)).
                Dot(
                    theta).
                Add(
                    W ??= Matrix.Identity(colBCount * P).Multiply(0.0001)
                )
                .
                Multiply(2);
        double[] F = [..
                (E.
                Multiply(
                    2).
                TransposeAndDot(
                    Q).
                Dot(
                    theta))];

        if (ConstantA == null)
        {
            ConstantA ??= new double[colBCount * P * 2, colBCount * P];
            ConstantB ??= new double[colBCount * P * 2];

            for (int i = 0; i < colBCount * P; i++)
            {
                ConstantA[i, i] = 1;
                ConstantA[i + colBCount * P, i] = -1;
                ConstantB[i] = uMin;
                ConstantB[i + colBCount * P] = -uMax;
            }
        }
        var quadratic = new QuadraticObjectiveFunction(H, F);
        var constraintCollection = LinearConstraintCollection.Create(
            ConstantA,
            ConstantB,
           0
           );

        GoldfarbIdnani solver = new(quadratic, constraintCollection);
        solver.Minimize();
        var uOut = solver.Solution[0..colBCount];
        if (colBCount == 2)
            return new Vector3d(uOut[0], uOut[1], 0);
        else
            return new Vector3d(uOut[0], uOut[1], uOut[2]);
    }
}