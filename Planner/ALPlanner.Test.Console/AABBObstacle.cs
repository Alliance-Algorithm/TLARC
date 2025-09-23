using System.Numerics;
using ALPlanner.Infrastructure.Optimizer;
using Kernel.Core.EventBus;
using Kernel.DataInterfaces;
using Kernel.DataInterfaces.Constraints;
using Kernel.DataInterfaces.Navigation;
using Kernel.DataInterfaces.Visualization;
using MathNet.Numerics.Optimization;
using NumFlat;

static class AABBObstacle
{
    public static List<AABB2D> GenerateCorridor()
    {
        var corridor = new List<AABB2D>();

        // 水平段：从 (0,0) 到 (10,2)，每段宽度为2
        for (int i = 0; i < 5; i++)
        {
            float x = i * 2;
            corridor.Add(new AABB2D(x, 0, x + 3, 2));
        }

        // 转弯段：连接水平和垂直段的拐角
        corridor.Add(new AABB2D(10, 1, 13, 3)); // 拐角段

        // 垂直段：从 (120,0) 向上延伸到 (120,100)，每段高度为20
        for (int i = 1; i <= 5; i++)
        {
            float y = i * 2;
            corridor.Add(new AABB2D(12, y, 14, y + 3));
        }

        return corridor;
    }

    static AABB2D[] path = [.. GenerateCorridor()];
    class PathDecorator<T>(T path) : IPath2D, IHeader where T : IEnumerable<Vector2>
    {
        public IHeader Header => this;

        public int Length => path.Count();

        public string Identifier => "Test";

        public IEnumerable<Vector2> GetPoints() => path;
    }
    class CorridorDecorator(AABB2D[] path) : ISafeCorridor2DData<IRectangle>, IHeader
    {
        public IHeader Header => this;

        public int Length => path.Length;

        public string Identifier => "Test";

        public IRectangle[] Corridors => [.. from a in path select a as IRectangle];

    }


    public static void Build()
    {
        int K = 1;
        Console.WriteLine(2 * 3 * path.Length * K);

        var ros = TlarcRosBridge.Domain.RosBridge.Build("MincoTest");
        ros.Publish<IPath2D, TlarcRosBridge.Infrastructure.Messages.Nav.Path>("/traj", "/traj"
                            , TlarcRosBridge.Infrastructure.DataProcess.Publisher.PublishPath);
        ros.Publish<IPath2D, TlarcRosBridge.Infrastructure.Messages.Nav.Path>("/path", "/path"
        , TlarcRosBridge.Infrastructure.DataProcess.Publisher.PublishPath);
        ros.Publish<ISafeCorridor2DData<IRectangle>, TlarcRosBridge.Infrastructure.Messages.Visualization.MarkerArray>("/safeCorridor", "/safeCorridor"
        , TlarcRosBridge.Infrastructure.DataProcess.Publisher.PublishRectangleSafeCorridor);

        Vector2 head = new(1, 1);
        Vector2 headv = new(0, 0);
        Vector2 heada = new(0, 0);
        var ts = 0f;

        Minco<AABB2D> minco = new(path.Length * K, path);
        var now = DateTime.UtcNow;
        var total = DateTime.UtcNow;
        var minimizer = new LimitedMemoryBfgsMinimizer(1e-6, 1e-6, 1e-6, 10 * 1024 * 1024, 1000);
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
                minco._tailPVA[0] = new(13, 12);
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
            EventBus<IPath2D>.Instance.Publish("/traj", new PathDecorator<IEnumerable<Vector2>>(recordMinco.GetPositions(ts, (minco.TotalSecond - ts) / 100, 101)));
            EventBus<IPath2D>.Instance.Publish("/path", new PathDecorator<IEnumerable<Vector2>>(recordMinco.GetControlPoints()));
            EventBus<ISafeCorridor2DData<IRectangle>>.Instance.Publish("/safeCorridor", new CorridorDecorator(path));

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
        } while (false);

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
