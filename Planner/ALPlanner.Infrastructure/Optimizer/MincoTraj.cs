
using Vectorf = NumFlat.Vec<double>;
using Matrixf = NumFlat.Mat<double>;
using System.Numerics;
using NumFlat;

namespace ALPlanner.Infrastructure.Optimizer;

internal class MincoTraj(in Matrixf c, in int N, in Vectorf T1, in Vectorf T2, in Vectorf T3, in Vectorf T4, in Vectorf T5, in Matrixf A,

 in Vector2[] headPVA, in Vector2[] tailPVA)
{
    internal readonly int N = N;
    internal const int S = Minco.S;
    internal readonly Matrixf b = new(2 * S * N, 2);
    readonly Matrixf c = c;
    readonly Vectorf T1 = T1;
    readonly Vectorf T2 = T2;
    readonly Vectorf T3 = T3;
    readonly Vectorf T4 = T4;
    readonly Vectorf T5 = T5;
    readonly Matrixf A = A;
    readonly Vector2[] headPVA = headPVA;
    readonly Vector2[] tailPVA = tailPVA;
    readonly Matrixf B1 = new(6, c.ColCount);
    readonly Matrixf B2 = new(3, c.ColCount);


    internal void Generate(in Matrixf innerPath)
    {
        if (N != 0)
        {
            T1.CopyTo(T2);
            T1.CopyTo(T3);
            T1.CopyTo(T4);
            T1.CopyTo(T5);
            T2.PointwiseMulInplace(T1);   // T^2
            T3.PointwiseMulInplace(T2);   // T^3
            T4.PointwiseMulInplace(T3);   // T^4
            T5.PointwiseMulInplace(T4);   // T^5

            // fill the F_0 = [\beta(0) \beta`1(0) \beta`2(0)]^T 3*6   
            // \beta(t)   = [0 t t^2 t^3  t^4  t^5 ]^T    6*1
            // \beta`1(t) = [0 1 2t  3t^2 4t^3  5t^4]^T
            // \beta`2(t) = [0 0 2   6t   12t^2 20t^3]^T
            A[0, 0] = 1.0;  // The first row of M matrix is, \beta(0) = [1 0 0 0 0 0]
            A[1, 1] = 1.0;  // The second row of M matrix is,\beta`1(0) = [0 1 0 0 0 0]
            A[2, 2] = 2.0;  // The third row of M matrix is, \beta`2(0) = [0 0 2 0 0 0]
            b[0, 0] = headPVA[0].X; b[0, 1] = headPVA[0].Y;  // pos for xyz
            b[1, 0] = headPVA[1].X; b[1, 1] = headPVA[1].Y;  // vel for xyz
            b[2, 0] = headPVA[2].X; b[2, 1] = headPVA[2].Y; // acc for xyz

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
                b[6 * i + 5, 0] = innerPath[i, 0];
                b[6 * i + 5, 1] = innerPath[i, 1]; // acc for xyz
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

        b[6 * N - 3, 0] = tailPVA[0].X; b[6 * N - 3, 1] = tailPVA[0].Y;  // pos for xyz
        b[6 * N - 2, 0] = tailPVA[1].X; b[6 * N - 2, 1] = tailPVA[1].Y;  // vel for xyz
        b[6 * N - 1, 0] = tailPVA[2].X; b[6 * N - 1, 1] = tailPVA[2].Y; // acc for xyz


        // solve the Ax=b function with A^-1 calculate by factorizeLU(). 

        A.Lu().Solve(b, c);

        return;
    }

    internal void AddPropCtoP(in Matrixf G, in Matrixf gQ)
    {
        // According to Eq.(8), the result is just the corresponding row of G
        // According to the A,b matrix layout in the program, the non-zero value 6i+5
        for (int i = 0; i < N - 1; i++)
            G.Rows[6 * i + 5].CopyTo(gQ.Rows[i]);
        return;
    }

    internal void AddPropCtoT(in Matrixf G, in Vectorf gT)
    {
        for (int i = 0; i < N - 1; i++)
        {
            // calculate \frac{\partial E_i}{\partial T_i}. E_i is 6*6 matrix
            // According the matrix layout, E_i = [\beta`3(T) \beta`4(T) \beta(T) \beta(T) \beta`1(T) \beta`2]^T
            // so \frac{\partial E_i}{\partial T_i} = [\beta`4(T) \beta`5(T) \beta`1(T) \beta`1(T) \beta`2(T) \beta`3]^T
            // multiply by c_i and get the [Snap, Crk, Vel, Vel, Acc, Jerk]

            var negSnp = B1.Rows[0];
            var negCrk = B1.Rows[1];
            var negVel = B1.Rows[2];
            var negAcc = B1.Rows[4];
            var negJer = B1.Rows[5];

            c.Rows[i * 6 + 1].CopyTo(negVel);
            negVel.MulInplace(-1);
            negVel.AddInplace(-2.0 * T1[i] * c.Rows[i * 6 + 2]);
            negVel.AddInplace(-3.0 * T2[i] * c.Rows[i * 6 + 3]);
            negVel.AddInplace(-4.0 * T3[i] * c.Rows[i * 6 + 4]);
            negVel.AddInplace(-5.0 * T4[i] * c.Rows[i * 6 + 5]);
            negVel.CopyTo(B1.Rows[3]);

            c.Rows[i * 6 + 2].CopyTo(negAcc);
            negAcc.MulInplace(-2.0);
            negAcc.AddInplace(-6.0 * T1[i] * c.Rows[i * 6 + 3]);
            negAcc.AddInplace(-12.0 * T2[i] * c.Rows[i * 6 + 4]);
            negAcc.AddInplace(-20.0 * T3[i] * c.Rows[i * 6 + 5]);

            c.Rows[i * 6 + 3].CopyTo(negJer);
            negJer.MulInplace(-6.0);
            negJer.AddInplace(-24.0 * T1[i] * c.Rows[i * 6 + 4]);
            negJer.AddInplace(-60.0 * T2[i] * c.Rows[i * 6 + 5]);

            c.Rows[i * 6 + 4].CopyTo(negSnp);
            negSnp.MulInplace(-24.0);
            negSnp.AddInplace(-120.0 * T1[i] * c.Rows[i * 6 + 5]);

            c.Rows[i * 6 + 5].CopyTo(negCrk);
            negCrk.MulInplace(-120.0);

            // Eq.(10). 
            B1.PointwiseMulInplace(G.Submatrix(6 * i + 3, 0, 6, 2));
            gT[i] += B1.Sum(x => x.Sum());
        }
        {
            var negVel = B2.Rows[0];
            var negAcc = B2.Rows[1];
            var negJer = B2.Rows[2];
            // The E_M should be specially handled because it's 3*6

            c.Rows[6 * N - 5].CopyTo(negVel);
            negVel.MulInplace(-1);
            negVel.AddInplace(-2.0 * T1[N - 1] * c.Rows[6 * N - 4]);
            negVel.AddInplace(-3.0 * T2[N - 1] * c.Rows[6 * N - 3]);
            negVel.AddInplace(-4.0 * T3[N - 1] * c.Rows[6 * N - 2]);
            negVel.AddInplace(-5.0 * T4[N - 1] * c.Rows[6 * N - 1]);

            c.Rows[6 * N - 4].CopyTo(negAcc);
            negAcc.MulInplace(-2.0);
            negAcc.AddInplace(-6.0 * T1[N - 1] * c.Rows[6 * N - 3]);
            negAcc.AddInplace(-12.0 * T2[N - 1] * c.Rows[6 * N - 2]);
            negAcc.AddInplace(-20.0 * T3[N - 1] * c.Rows[6 * N - 1]);

            c.Rows[6 * N - 3].CopyTo(negJer);
            negJer.MulInplace(-6.0);
            negJer.AddInplace(-24.0 * T1[N - 1] * c.Rows[6 * N - 2]);
            negJer.AddInplace(-60.0 * T2[N - 1] * c.Rows[6 * N - 1]);

            B2.PointwiseMulInplace(G.Submatrix(6 * N - 3, 0, 3, 2));
            gT[N - 1] += B2.Sum(x => x.Sum());
        }
        return;
    }

}