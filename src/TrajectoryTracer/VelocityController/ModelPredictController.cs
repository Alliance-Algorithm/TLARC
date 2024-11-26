using Accord.Math.Optimization;
using TlarcKernel;
namespace TrajectoryTracer.Controller;

class VelocityMPC : Component, IPositionVelocityController
{
    [ComponentReferenceFiled]
    ICarModel car;
    private int ControlPredictLength = 40;
    public int P => ControlPredictLength;
    public double Q = 20;
    public double R = 1;
    public double rho = 10;
    public double[] U;
    public double[] uOut;
    public const double uMax = 5;
    public const double uMin = -uMax;

    LinearConstraintCollection constraintCollection;
    public Vector3d ControlVolume(Vector3d Position)
    {
        if (U == null)
            U = Vector.Zeros(car.ControlVolumeSize);
        if (uOut == null)
            uOut = Vector.Zeros(car.ControlVolumeSize);
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

        for (int i = 0; i < car.ControlVolumeSize * P; i++)
        {
            var ConstantA = new double[car.ControlVolumeSize * P];
            ConstantA[i] = 1;
            constraintCollection.Add(new(car.ControlVolumeSize * P)
            {
                CombinedAs = ConstantA,
                Value = uMin - U[i % 2],
                ShouldBe = ConstraintType.GreaterThanOrEqualTo
            });
            constraintCollection.Add(new(car.ControlVolumeSize * P)
            {
                CombinedAs = ConstantA,
                Value = uMax - U[i % 2],
                ShouldBe = ConstraintType.LesserThanOrEqualTo
            });
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