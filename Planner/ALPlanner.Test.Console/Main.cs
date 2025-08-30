using System.Numerics;
using ALPlanner.Infrastructure.Optimizer;
using Kernel.Core.EventBus;
using Kernel.DataInterfaces;
using Kernel.DataInterfaces.Constraints;
using Kernel.DataInterfaces.Navigation;
using MathNet.Numerics.Optimization;
using NumFlat;

static class Program
{

    readonly static Circle2D[] path = [
        new(1.0f, new(0,0)),
        new(0.8f,new(1f,1f)),
        new(0.8f,new(2f,2f)),
        new(0.5f,new(2.5f,3f)),
        new(0.4f,new(3f,3.4f)),
        new(0.4f,new(3.0f,4.0f)),
        new(0.4f,new(2.6f,4.2f)),
        new(0.4f,new(2.0f,4.4f)),
        new(0.5f,new(1.3f,4.1f)),
        new(0.5f,new(0.7f,3.5f)),
        new(0.6f,new(0.2f,3.0f)),
        new(0.7f,new(-0.5f,2.7f)),
        new(0.8f,new(-1.5f,2.5f)),
    ];
    class PathDecorator<T>(T path) : IPath2D, IHeader where T : IEnumerable<Vector2>
    {
        public IHeader Header => this;

        public int Length => path.Count();

        public string Identifier => "Test";

        public IEnumerable<Vector2> GetPoints() => path;
    }
    class CorridorDecorator<T>(T[] path) : ISafeCorridor2D<Circle2D>, IHeader where T : Circle2D
    {
        public IHeader Header => this;

        public int Length => path.Length;

        public string Identifier => "Test";

        public Circle2D[] Corridors => path;

    }


    static void Main(string[] paras)
    {
        var now = DateTime.UtcNow;
        int K = 1;
        Minco minco = new(path.Length * K, path);
        minco._headPVA[0] = new(0, 0);
        minco._headPVA[1] = new(0, 0);
        minco._headPVA[2] = new(0, 0);
        minco._tailPVA[0] = new(-1.5f, 2.5f);
        minco._tailPVA[1] = new(0, 0);
        minco._tailPVA[2] = new(0, 0);

        Console.WriteLine(2 * 3 * path.Length * K);


        var func = ObjectiveFunction.Gradient(x =>
        {
            var arr = x.ToArray().AsMemory();
            var XVec = new Vec<double>(arr);
            minco.Generate(XVec[..(path.Length * K)], XVec[(path.Length * K)..]);
            var a = minco.GetJ();
            var b = MathNet.Numerics.LinearAlgebra.Vector<double>.Build.DenseOfArray([.. minco.GetGradient(XVec[(path.Length * K)..])]);

            return (a, b);

        });
        var minimizer = new LimitedMemoryBfgsMinimizer(1e-6, 1e-6, 1e-6, 10 * 1024 * 1024, 100);
        var rst = minimizer.FindMinimum(func, MathNet.Numerics.LinearAlgebra.Vector<double>.Build.Dense(path.Length - 1 + (K - 1) * 2 * path.Length + path.Length * K));

        var x = rst.MinimizingPoint;
        var arr = x.ToArray().AsMemory();
        var XVec = new Vec<double>(arr);
        var tau = XVec[..(path.Length * K)];
        var kesi = XVec[(path.Length * K)..];
        minco.Generate(tau, kesi);
        var t = DateTime.UtcNow - now;
        Console.WriteLine($"{rst.Iterations},{t.TotalMilliseconds}");

        // static void func(double[] x, ref double func, double[] grad, object obj)
        // {
        //     var (minco, K) = ((Minco, int))obj;
        //     var xVec = new Vec<double>(x.AsMemory());
        //     minco.Generate(xVec[..(path.Length * K)], xVec[(path.Length * K)..]);
        //     func = minco.GetJ();
        //     minco.GetGradient(xVec[(path.Length * K)..]).CopyTo(grad);
        // }
        // for (int i = 0; i < 100; i++)
        // {
        //     var now = DateTime.UtcNow;
        //     var n = (path.Length - 1) + (K - 1) * 2 * path.Length + path.Length * K;
        //     alglib.minlbfgscreate(n, 4, new double[n], out var lbfgsState);
        //     alglib.minlbfgsoptimize(lbfgsState, func, null, (minco, K));
        //     var xVec = new Vec<double>(lbfgsState.x.AsMemory());

        //     var tau = xVec[..(path.Length * K)];
        //     var kesi = xVec[(path.Length * K)..];
        // }

        var ros = TlarcRosBridge.Domain.RosBridge.Build("MincoTest");
        ros.Publish<IPath2D, TlarcRosBridge.Infrastructure.Messages.Nav.Path>("/traj", "/traj"
                            , TlarcRosBridge.Infrastructure.DataProcess.Publisher.PublishPath);
        ros.Publish<IPath2D, TlarcRosBridge.Infrastructure.Messages.Nav.Path>("/path", "/path"
        , TlarcRosBridge.Infrastructure.DataProcess.Publisher.PublishPath);
        ros.Publish<ISafeCorridor2D<Circle2D>, TlarcRosBridge.Infrastructure.Messages.Visualization.MarkerArray>("/safeCorridor", "/safeCorridor"
        , TlarcRosBridge.Infrastructure.DataProcess.Publisher.PublishSafeCorridor);
        EventBus<IPath2D>.Instance.Publish("/traj", new PathDecorator<IEnumerable<Vector2>>(minco.GetPositions(0, minco.TotalSecond / 100, 101)));
        EventBus<IPath2D>.Instance.Publish("/path", new PathDecorator<IEnumerable<Vector2>>(minco.GetControlPoints()));
        EventBus<ISafeCorridor2D<Circle2D>>.Instance.Publish("/safeCorridor", new CorridorDecorator<Circle2D>(path));
        Console.WriteLine(minco.TotalSecond);


        ScottPlot.Plot myPlot = new();

        var velocity = minco.GetVelocitys(0, minco.TotalSecond / 100, 101);
        float[] dataY = [.. from v in velocity select v.Length()];
        float[] dataX = new float[dataY.Length];
        for (int i = 0; i < dataY.Length; i++)
            dataX[i] = i;
        myPlot.Add.Scatter(dataX, dataY);

        velocity = minco.GetAccelerates(0, minco.TotalSecond / 100, 101);
        dataY = [.. from v in velocity select v.Length()];
        dataX = new float[dataY.Length];
        for (int i = 0; i < dataY.Length; i++)
            dataX[i] = i;
        myPlot.Add.Scatter(dataX, dataY);

        myPlot.SavePng("quickstart.png", 1000, 1000);
    }
}
