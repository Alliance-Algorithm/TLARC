using System.Numerics;
using ALPlanner.Domain.Trajectorys;
using Kernel.DataInterfaces.Constraints;
using Kernel.DataInterfaces.Navigation;
using Kernel.DataInterfaces.Visualization;
using MathNet.Numerics.Optimization;
using NumFlat;

namespace ALPlanner.Infrastructure.Optimizer;

public static class MincoOptimizer
{
    static readonly LimitedMemoryBfgsMinimizer minimizer = new(1e-6, 1e-6, 1e-6, 10 * 1024 * 1024, 100);
    public record Status(Vector2 Pos, Vector2 Vel, Vector2 Acc);
    static readonly int K = 1;

    public static ITrajectory2D? Optimize<T>(ISafeCorridor2DData<T> path, Status head, Status tail) where T : IConstraint
    {
        try
        {
            DateTime now = DateTime.UtcNow;

            Minco<T> minco = new(path.Length * K, path.Corridors);

            var func = ObjectiveFunction.Gradient(x =>
            {
                var arr = x.ToArray().AsMemory();
                var XVec = new Vec<double>(arr);
                minco.Generate(XVec[..(path.Length * K)], XVec[(path.Length * K)..]);
                var a = minco.GetJ();
                var b = MathNet.Numerics.LinearAlgebra.Vector<double>.Build.DenseOfArray([.. minco.GetGradient(XVec[(path.Length * K)..])]);

                return (a, b);
            });
            minco._headPVA[0] = head.Pos;
            minco._headPVA[1] = head.Vel;
            minco._headPVA[2] = head.Acc;
            minco._tailPVA[0] = tail.Pos;
            minco._tailPVA[1] = tail.Vel;
            minco._tailPVA[2] = tail.Acc;

            var rst = minimizer.FindMinimum(func, MathNet.Numerics.LinearAlgebra.Vector<double>.Build.Dense(minco.XSize, Minco<T>.Init));

            var x = rst.MinimizingPoint;
            // var x = MathNet.Numerics.LinearAlgebra.Vector<double>.Build.Dense(path.Length - 1 + (K - 1) * 2 * path.Length + path.Length * K);
            var arr = x.ToArray().AsMemory();
            var XVec = new Vec<double>(arr);
            var tau = XVec[..(path.Length * K)];
            var kesi = XVec[(path.Length * K)..];
            minco.Generate(tau, kesi);

            return new MincoTrajectory(minco.Record) { Header = path.Header, FromWhen = now };
        }
        catch (Exception e) { Console.WriteLine(e.Message); return null; }
    }
}