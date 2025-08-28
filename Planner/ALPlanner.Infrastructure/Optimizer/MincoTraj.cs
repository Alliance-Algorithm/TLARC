
using Vectord = MathNet.Numerics.LinearAlgebra.Vector<double>;
using Matrixd = MathNet.Numerics.LinearAlgebra.Matrix<double>;
using System.Numerics;

namespace ALPlanner.Infrastructure.Optimizer;

public class MincoTraj(in Matrixd c, in int N, in Vectord T1, in Vectord T2, in Vectord T3, in Vectord T4, in Vectord T5, in Matrixd A,

 in Vector2[] headPVA, in Vector2[] tailPVA)
{
    internal readonly int N = N;
    internal const int S = Minco.S;
    internal readonly Matrixd b = Matrixd.Build.Dense(2 * S * N, 2);
    readonly Matrixd c = c;
    readonly Vectord T1 = T1;
    readonly Vectord T2 = T2;
    readonly Vectord T3 = T3;
    readonly Vectord T4 = T4;
    readonly Vectord T5 = T5;
    readonly Matrixd A = A;
    readonly Vector2[] headPVA = headPVA;
    readonly Vector2[] tailPVA = tailPVA;


    internal void Generate(in Matrixd innerPath)
    {
        if (N != 0)
        {
            T1.PointwiseMultiply(T1, T2);   // T^2
            T2.PointwiseMultiply(T1, T3);   // T^3
            T2.PointwiseMultiply(T2, T4);   // T^4
            T4.PointwiseMultiply(T1, T5);   // T^5

            // fill the F_0 = [\beta(0) \beta`1(0) \beta`2(0)]^T 3*6   
            // \beta(t)   = [0 t t^2 t^3  t^4  t^5 ]^T    6*1
            // \beta`1(t) = [0 1 2t  3t^2 4t^3  5t^4]^T
            // \beta`2(t) = [0 0 2   6t   12t^2 20t^3]^T
            A[0, 0] = 1.0;  // The first row of M matrix is, \beta(0) = [1 0 0 0 0 0]
            A[1, 1] = 1.0;  // The second row of M matrix is,\beta`1(0) = [0 1 0 0 0 0]
            A[2, 2] = 2.0;  // The third row of M matrix is, \beta`2(0) = [0 0 2 0 0 0]
            b.SetRow(0, [headPVA[0].X, headPVA[0].Y]);  // pos for xyz
            b.SetRow(1, [headPVA[1].X, headPVA[1].Y]);  // vel for xyz
            b.SetRow(2, [headPVA[2].X, headPVA[2].Y]); // acc for xyz

            for (int i = 0; i < N - 1; i++)
            {
                // The fifth row is \beta`3(T)*c_{i} - \beta(0)`3*c_{i+1} = 0  
                // because of 3 order continuty condition                    
                A[6 * i + 3, 6 * i + 3] = 6.0;
                A[6 * i + 3, 6 * i + 4] = 24.0 * T1[i];
                A[6 * i + 3, 6 * i + 5] = 60.0 * T2[i];
                A[6 * i + 3, 6 * i + 9] = -6.0;
                // The sixth row is \beta`4(T)*c_{i} - \beta(0)`4*c_{i+1} = 0  
                // because of 3 order continuty condition                    
                A[6 * i + 4, 6 * i + 4] = 24.0;
                A[6 * i + 4, 6 * i + 5] = 120.0 * T1[i];
                A[6 * i + 4, 6 * i + 10] = -24.0;

                // The first row is \beta(T)*c_i = given point
                A[6 * i + 5, 6 * i] = 1.0;
                A[6 * i + 5, 6 * i + 1] = T1[i];
                A[6 * i + 5, 6 * i + 2] = T2[i];
                A[6 * i + 5, 6 * i + 3] = T3[i];
                A[6 * i + 5, 6 * i + 4] = T4[i];
                A[6 * i + 5, 6 * i + 5] = T5[i];

                // The second row is \beta(T)*c_{i} - \beta(0)*c_{i+1} = 0  
                // because of 0 order continuty condition
                A[6 * i + 6, 6 * i] = 1.0;
                A[6 * i + 6, 6 * i + 1] = T1[i];
                A[6 * i + 6, 6 * i + 2] = T2[i];
                A[6 * i + 6, 6 * i + 3] = T3[i];
                A[6 * i + 6, 6 * i + 4] = T4[i];
                A[6 * i + 6, 6 * i + 5] = T5[i];
                A[6 * i + 6, 6 * i + 6] = -1.0;
                // The third row is \b]ta`1(T)*c_{i} - \beta`1(0)*c_{i+1} = 0  
                // because of 1 order continuty condition
                A[6 * i + 7, 6 * i + 1] = 1.0;
                A[6 * i + 7, 6 * i + 2] = 2 * T1[i];
                A[6 * i + 7, 6 * i + 3] = 3 * T2[i];
                A[6 * i + 7, 6 * i + 4] = 4 * T3[i];
                A[6 * i + 7, 6 * i + 5] = 5 * T4[i];
                A[6 * i + 7, 6 * i + 7] = -1.0;
                // The fourth row is \beta`2(T)*c_{i} - \beta`2(0)*c_{i+1} = 0  
                // because of 2 order continuty condition
                A[6 * i + 8, 6 * i + 2] = 2.0;
                A[6 * i + 8, 6 * i + 3] = 6 * T1[i];
                A[6 * i + 8, 6 * i + 4] = 12 * T2[i];
                A[6 * i + 8, 6 * i + 5] = 20 * T3[i];
                A[6 * i + 8, 6 * i + 8] = -2.0;
                // fill the b with MINCO mid point
                b.SetRow(6 * i + 5, [innerPath[i, 0], innerPath[i, 1]]);
            }
        }
        A[6 * N - 3, 6 * N - 6] = 1.0;
        A[6 * N - 3, 6 * N - 5] = T1[N - 1];
        A[6 * N - 3, 6 * N - 4] = T2[N - 1];
        A[6 * N - 3, 6 * N - 3] = T3[N - 1];
        A[6 * N - 3, 6 * N - 2] = T4[N - 1];
        A[6 * N - 3, 6 * N - 1] = T5[N - 1];
        A[6 * N - 2, 6 * N - 5] = 1.0;
        A[6 * N - 2, 6 * N - 4] = 2 * T1[N - 1];
        A[6 * N - 2, 6 * N - 3] = 3 * T2[N - 1];
        A[6 * N - 2, 6 * N - 2] = 4 * T3[N - 1];
        A[6 * N - 2, 6 * N - 1] = 5 * T4[N - 1];
        A[6 * N - 1, 6 * N - 4] = 2;
        A[6 * N - 1, 6 * N - 3] = 6 * T1[N - 1];
        A[6 * N - 1, 6 * N - 2] = 12 * T2[N - 1];
        A[6 * N - 1, 6 * N - 1] = 20 * T3[N - 1];
        // fill the b with the MINCO end state
        b.SetRow(6 * N - 3, [tailPVA[0].X, tailPVA[0].Y]);
        b.SetRow(6 * N - 2, [tailPVA[1].X, tailPVA[1].Y]);
        b.SetRow(6 * N - 1, [tailPVA[2].X, tailPVA[2].Y]);


        // solve the Ax=b function with A^-1 calculate by factorizeLU(). 
        A.LU().Solve(b, c);

        return;
    }

    internal void AddPropCtoP(in Matrixd G, in Matrixd gQ)
    {
        // According to Eq.(8), the result is just the corresponding row of G
        // According to the A,b matrix layout in the program, the non-zero value 6i+5
        for (int i = 0; i < N - 1; i++)
            gQ.SetRow(i, G.Row(6 * i + 5));
        return;
    }

    internal void AddPropCtoT(in Matrixd G, in Vectord gT)
    {
        for (int i = 0; i < N - 1; i++)
        {
            // calculate \frac{\partial E_i}{\partial T_i}. E_i is 6*6 matrix
            // According the matrix layout, E_i = [\beta`3(T) \beta`4(T) \beta(T) \beta(T) \beta`1(T) \beta`2]^T
            // so \frac{\partial E_i}{\partial T_i} = [\beta`4(T) \beta`5(T) \beta`1(T) \beta`1(T) \beta`2(T) \beta`3]^T
            // multiply by c_i and get the [Snap, Crk, Vel, Vel, Acc, Jerk]
            var negVel = -(c.Row(i * 6 + 1) +
                        2.0 * T1[i] * c.Row(i * 6 + 2) +
                        3.0 * T2[i] * c.Row(i * 6 + 3) +
                        4.0 * T3[i] * c.Row(i * 6 + 4) +
                        5.0 * T4[i] * c.Row(i * 6 + 5));
            var negAcc = -(2.0 * c.Row(i * 6 + 2) +
                         6.0 * T1[i] * c.Row(i * 6 + 3) +
                         12.0 * T2[i] * c.Row(i * 6 + 4) +
                         20.0 * T3[i] * c.Row(i * 6 + 5));
            var negJer = -(6.0 * c.Row(i * 6 + 3) +
                         24.0 * T1[i] * c.Row(i * 6 + 4) +
                         60.0 * T2[i] * c.Row(i * 6 + 5));
            var negSnp = -(24.0 * c.Row(i * 6 + 4) +
                         120.0 * T1[i] * c.Row(i * 6 + 5));
            var negCrk = -120.0 * c.Row(i * 6 + 5);

            var B1 = Matrixd.Build.DenseOfRowVectors(negSnp, negCrk, negVel, negVel, negAcc, negJer);
            // Eq.(10). 
            gT[i] += B1.PointwiseMultiply(G.SubMatrix(6 * i + 3, 6, 0, 2)).RowSums().Sum();
        }
        {
            // The E_M should be specially handled because it's 3*6
            var negVel = -(c.Row(6 * N - 5) +
                                  2.0 * T1[N - 1] * c.Row(6 * N - 4) +
                                  3.0 * T2[N - 1] * c.Row(6 * N - 3) +
                                  4.0 * T3[N - 1] * c.Row(6 * N - 2) +
                                  5.0 * T4[N - 1] * c.Row(6 * N - 1));
            var negAcc = -(2.0 * c.Row(6 * N - 4) +
                          6.0 * T1[N - 1] * c.Row(6 * N - 3) +
                          12.0 * T2[N - 1] * c.Row(6 * N - 2) +
                          20.0 * T3[N - 1] * c.Row(6 * N - 1));
            var negJer = -(6.0 * c.Row(6 * N - 3) +
                           24.0 * T1[N - 1] * c.Row(6 * N - 2) +
                           60.0 * T2[N - 1] * c.Row(6 * N - 1));
            var B2 = Matrixd.Build.DenseOfRowVectors(negVel, negAcc, negJer);
            gT[N - 1] += B2.PointwiseMultiply(G.SubMatrix(6 * N - 3, 3, 0, 2)).RowSums().Sum();
        }
        return;
    }

}