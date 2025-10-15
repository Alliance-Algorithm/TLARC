using System.Numerics;
using ALPlanner.Infrastructure.Optimizer;
using Kernel.Core.EventBus;
using Kernel.Contract;
using Kernel.Contract.Constraints;
using Kernel.Contract.Navigation;
using Kernel.Contract.Visualization;
using MathNet.Numerics.Optimization;
using NumFlat;

static class CircleObstacle
{

    static Circle[] path = [
        new Circle{R = 1.0f, Origin = new(0,0)},
        new Circle{R =0.8f,Origin = new(1f,1f)},
        new Circle{R =0.8f,Origin = new(2f,2f)},
        new Circle{R =0.5f,Origin = new(2.5f,3f)},
        new Circle{R =0.4f,Origin = new(3f,3.4f)},
        new Circle{R =0.4f,Origin = new(3.0f,4.0f)},
        new Circle{R =0.4f,Origin = new(2.6f,4.2f)},
        new Circle{R =0.4f,Origin = new(2.0f,4.4f)},
        new Circle{R =0.5f,Origin = new(1.3f,4.1f)},
        new Circle{R =0.5f,Origin = new(0.7f,3.5f)},
        new Circle{R =0.6f,Origin = new(0.2f,3.0f)},
        new Circle{R =0.7f,Origin = new(-0.5f,2.7f)},
        new Circle{R =0.8f,Origin = new(-1.5f,2.5f)},
    ];


    public static void Build()
    {
        int K = 1;
        Console.WriteLine(2 * 3 * path.Length * K);

        var ros = TlarcRosBridge.Domain.RosBridge.Build("MincoTest");
        ros.Publish<Path2D, TlarcRosBridge.Infrastructure.Messages.Nav.Path>("/traj", "/traj"
                            , TlarcRosBridge.Infrastructure.DataProcess.Publisher.PublishPath);
        ros.Publish<Path2D, TlarcRosBridge.Infrastructure.Messages.Nav.Path>("/path", "/path"
        , TlarcRosBridge.Infrastructure.DataProcess.Publisher.PublishPath);
        ros.Publish<SafeCorridor2DData<Circle>, TlarcRosBridge.Infrastructure.Messages.Visualization.MarkerArray>("/safeCorridor", "/safeCorridor"
        , TlarcRosBridge.Infrastructure.DataProcess.Publisher.PublishCircleSafeCorridor);

        Vector2 head = new(0, 0);
        Vector2 headv = new(0, 0);
        Vector2 heada = new(0, 0);
        var ts = 0f;

        Minco<Circle> minco = new(path.Length * K, path);
        var now = DateTime.UtcNow;
        var total = DateTime.UtcNow;
        var minimizer = new LimitedMemoryBfgsMinimizer(1e-6, 1e-6, 1e-6, 10 * 1024 * 1024, 100);
        do
        {
            if (ts == 0)
            {
                minco = new(path.Length * K, path);

                var func = ObjectiveFunction.Gradient(x =>
                {
                    var arr = x.ToArray().AsMemory();
                    var XVec = new Vec<double>(arr);
                    minco.Generate(XVec[..(path.Length * K)], XVec[(path.Length * K)..]);
                    var a = minco.GetJ();
                    var b = MathNet.Numerics.LinearAlgebra.Vector<double>.Build.DenseOfArray([.. minco.GetGradient(XVec[(path.Length * K)..])]);

                    return (a, b);
                });
                minco._headPVA[0] = head;
                minco._headPVA[1] = headv;
                minco._headPVA[2] = heada;
                minco._tailPVA[0] = new(-1.5f, 2.5f);
                minco._tailPVA[1] = new(0, 0);
                minco._tailPVA[2] = new(0, 0);
                var rst = minimizer.FindMinimum(func, MathNet.Numerics.LinearAlgebra.Vector<double>.Build.Dense(path.Length - 1 + (K - 1) * 2 * path.Length + path.Length * K));

                var x = rst.MinimizingPoint;
                var arr = x.ToArray().AsMemory();
                var XVec = new Vec<double>(arr);
                var tau = XVec[..(path.Length * K)];
                var kesi = XVec[(path.Length * K)..];
                minco.Generate(tau, kesi);
                // Console.WriteLine(minco.TotalSecond);
            }
            ts = (float)(DateTime.UtcNow - now).TotalSeconds;
            var recordMinco = minco.Record;
            EventBus<Path2D>.Instance.Publish("/traj", new Path2D{Points = [.. recordMinco.GetPositions(ts, (minco.TotalSecond - ts) / 100, 101)], Header = new Header{Identifier = "test"}});
            EventBus<Path2D>.Instance.Publish("/path", new Path2D{Points = [.. recordMinco.GetControlPoints()], Header = new Header{Identifier = "test"}});
            EventBus<SafeCorridor2DData<Circle>>.Instance.Publish("/safeCorridor", new SafeCorridor2DData<Circle>{Corridors = path, Header = new Header{Identifier = "test"}});

            Thread.Sleep(50);
            head = recordMinco.GetPosition(ts);
            headv = recordMinco.GetVelocity(ts);
            heada = recordMinco.GetAccelerate(ts);
            if ((head - path[0].Origin).Length() < path[0].R)
            {
                path = path[1..];
                now = DateTime.UtcNow;
                ts = 0;
            }
            Console.WriteLine($"{minco.TotalSecond},{(DateTime.UtcNow - total).TotalSeconds}");
        } while (true);

        // ScottPlot.Plot myPlot = new();

        // var velocity = minco.GetVelocitys(0, minco.TotalSecond / 100, 101);
        // float[] dataY = [.. from v in velocity select v.Length()];
        // float[] dataX = new float[dataY.Length];
        // for (int i = 0; i < dataY.Length; i++)
        //     dataX[i] = i;
        // myPlot.Add.Scatter(dataX, dataY);

        // velocity = minco.GetAccelerates(0, minco.TotalSecond / 100, 101);
        // dataY = [.. from v in velocity select v.Length()];
        // dataX = new float[dataY.Length];
        // for (int i = 0; i < dataY.Length; i++)
        //     dataX[i] = i;
        // myPlot.Add.Scatter(dataX, dataY);

        // myPlot.SavePng("quickstart.png", 1000, 1000);
    }
}
