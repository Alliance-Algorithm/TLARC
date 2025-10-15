using System.Numerics;
using ALPlanner.Infrastructure.Optimizer;
using Kernel.Core.EventBus;
using Kernel.Contract;
using Kernel.Contract.Constraints;
using Kernel.Contract.Navigation;
using Kernel.Contract.Visualization;
using MathNet.Numerics.Optimization;
using NumFlat;
using ALPlanner.Infrastructure.SafeCorridorConstruct;

static class AABBObstacle
{
    public static List<AABB2D> GenerateCorridor()
    {
        var corridor = new List<AABB2D>();

        // 水平段：从 (0,0) 到 (10,2)，每段宽度为2
        for (int i = 0; i < 5; i++)
        {
            float x = i * 2;
            corridor.Add(new AABB2D{MaxX = x,MinX = x - 1,MaxY = x + 3, MinY = x + 2});
        }

        // 转弯段：连接水平和垂直段的拐角
        corridor.Add(new AABB2D{MaxX = 10,MinX = 9,MaxY = 13, MinY = 13}); // 拐角段

        // 垂直段：从 (120,0) 向上延伸到 (120,100)，每段高度为20
        for (int i = 1; i <= 5; i++)
        {
            float y = i * 2;
            corridor.Add(new AABB2D{MaxX = 10 - y,MinX = 10 + y,MaxY = 13 - y,MinY = y + 14});
        }

        return corridor;
    }

    static AABB2D[] path = [.. GenerateCorridor()];

    public static void Build()
    {
        int K = 1;
        Console.WriteLine(2 * 3 * path.Length * K);

        var ros = TlarcRosBridge.Domain.RosBridge.Build("MincoTest");
        ros.Publish<Path2D, TlarcRosBridge.Infrastructure.Messages.Nav.Path>("/traj", "/traj"
                            , TlarcRosBridge.Infrastructure.DataProcess.Publisher.PublishPath);
        ros.Publish<Path2D, TlarcRosBridge.Infrastructure.Messages.Nav.Path>("/path", "/path"
        , TlarcRosBridge.Infrastructure.DataProcess.Publisher.PublishPath);
        ros.Publish<SafeCorridor2DData<Rectangle>, TlarcRosBridge.Infrastructure.Messages.Visualization.MarkerArray>("/safeCorridor", "/safeCorridor"
        , TlarcRosBridge.Infrastructure.DataProcess.Publisher.PublishRectangleSafeCorridor);

        Vector2 head = new(1, 1);
        Vector2 headv = new(0, 0);
        Vector2 heada = new(0, 0);
        var ts = 0f;

        Minco<AABB2D> minco = new(path.Length * K, path);
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
                minco._tailPVA[0] = new(1.5f, 22);
                minco._tailPVA[1] = new(0, 0);
                minco._tailPVA[2] = new(0, 0);
                var rst = minimizer.FindMinimum(func, MathNet.Numerics.LinearAlgebra.Vector<double>.Build.Dense(minco.XSize, 1));

                var x = rst.MinimizingPoint;
                // var x = MathNet.Numerics.LinearAlgebra.Vector<double>.Build.Dense(minco.XSize, 0);
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
            // EventBus<SafeCorridor2DData<Rectangle>>.Instance.Publish("/safeCorridor", new SafeCorridorDecorator(path));

            Thread.Sleep(50);
            head = recordMinco.GetPosition(ts);
            headv = recordMinco.GetVelocity(ts);
            heada = recordMinco.GetAccelerate(ts);
            static bool check(Vector2 p, AABB2D aabb) => p.X < aabb.MaxX && p.Y < aabb.MaxY && p.Y > aabb.MinY && p.X > aabb.MinX;
            if (check(head, path[1]))
            {
                path = path[1..];
                now = DateTime.UtcNow;
                ts = 0;
            }
            Console.WriteLine($"{minco.TotalSecond},{(DateTime.UtcNow - total).TotalSeconds}");
        } while (true);

        ScottPlot.Plot myPlot = new();

        var velocity = minco.Record.GetVelocitys(0, minco.TotalSecond / 100, 101);
        float[] dataY = [.. from v in velocity select v.Length()];
        float[] dataX = new float[dataY.Length];
        for (int i = 0; i < dataY.Length; i++)
            dataX[i] = i;
        myPlot.Add.Scatter(dataX, dataY);

        velocity = minco.Record.GetAccelerates(0, minco.TotalSecond / 100, 101);
        dataY = [.. from v in velocity select v.Length()];
        dataX = new float[dataY.Length];
        for (int i = 0; i < dataY.Length; i++)
            dataX[i] = i;
        myPlot.Add.Scatter(dataX, dataY);

        myPlot.SavePng("quickstart.png", 1000, 1000);
    }
}
