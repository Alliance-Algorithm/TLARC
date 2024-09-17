
using Accord.Math;
using Accord.Math.Optimization;
using g4;
using Microsoft.Toolkit.HighPerformance;
using TlarcKernel;

namespace ALPlanner.TrajectoryOptimizer.Curves;

class FourthOrderNonUniformBSpline : Component, IKOrderCurve
{
    private double LooseSize = 0.15;
    const int order = 4;
    private double[] timeLine;
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
    private double[,] M4(int i)
    {
        var tmp = Math.Pow(timeLine[i + 1] - timeLine[i], 2);
        M4Data[0, 0] = tmp / (timeLine[i + 1] - timeLine[i - 1]) / (timeLine[i + 1] - timeLine[i - 2]);
        M4Data[0, 2] = tmp / (timeLine[i + 2] - timeLine[i - 1]) / (timeLine[i + 1] - timeLine[i - 1]);
        M4Data[1, 2] = 3 * (timeLine[i + 1] - timeLine[i]) * (timeLine[i] - timeLine[i - 1]) / (timeLine[i + 2] - timeLine[i - 1]) / (timeLine[i + 1] - timeLine[i - 1]);
        M4Data[2, 2] = 3 * tmp / (timeLine[i + 2] - timeLine[i - 1]) / (timeLine[i + 1] - timeLine[i - 1]);
        M4Data[3, 3] = tmp / (timeLine[i + 3] - timeLine[i]) / (timeLine[i + 2] - timeLine[i]);
        M4Data[3, 2] = -M4Data[2, 2] / 3 - M4Data[3, 3] - tmp / (timeLine[i + 2] - timeLine[i]) / (timeLine[i + 2] - timeLine[i - 1]);

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
            B[0][index + 5] = position.x + LooseSize;
            B[1][index + 5] = position.y + LooseSize;
            B[2][index + 5] = position.z + LooseSize;
            B[0][index + positionList.Count() - 2] = -position.x + LooseSize;
            B[1][index + positionList.Count() - 2] = -position.y + LooseSize;
            B[2][index + positionList.Count() - 2] = -position.z + LooseSize;
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
    }

    public IEnumerable<Vector3d> TrajectoryPoints(float fromWhen, float toWhen, float step)
    {
        throw new NotImplementedException();
    }

    public Vector3d Value(float time)
    {
        throw new NotImplementedException();
    }

    public override void Start()
    {
        for (double u = 0; u < 1; u += 0.1)
        {
            var k = new double[1, 4] { { 0, 0, 2, 6 * u / 100 } }.Dot(M4S);
            H4S = H4S.Add(k.TransposeAndDot(k));
        }
    }

}