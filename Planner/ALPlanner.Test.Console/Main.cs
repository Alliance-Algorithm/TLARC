using System.Numerics;
using ALPlanner.Infrastructure.Optimizer;
using Kernel.Core.EventBus;
using Kernel.DataInterfaces;
using Kernel.DataInterfaces.Constraints;
using Kernel.DataInterfaces.Navigation;
using MathNet.Numerics.Optimization;
static class Program
{
    class Obs : IObstacle
    {
        public bool FindObstacle(Vector2 from, float radius, out IEnumerable<Vector2> obstacles)
        {
            throw new NotImplementedException();
        }
    }

    readonly static Circle2D[] path = [
        new(1.0f, new(0,0)),
        new(1.0f,new(1f,0.8f)),
        new(1.0f,new(2.0f,1f)),
        new(1.0f,new(2.8f,1.8f)),
        new(1.0f,new(3.0f,2.6f)),
        new(1.0f,new(3.0f,3.5f)),
        new(1.2f,new(2.5f,4.5f)),
    ];
    class PathDecorator<T>(T path) : IPath2D, IHeader where T : IEnumerable<Vector2>
    {
        public IHeader Header => this;

        public int Length => path.Count();

        public string Identifier => "Test";

        public IEnumerable<Vector2> GetPoints() => path;
    }
    static void Main(string[] paras)
    {

        int K = 2;
        // path.AsParallel().ForAll(p => p /= 10);
        Minco minco = new(path.Length * K, path);
        minco._headPVA[0] = new(0, 0);
        minco._headPVA[1] = new(0, 0);
        minco._headPVA[2] = new(0, 0);
        minco._tailPVA[0] = new(2.5f, 4.5f);
        minco._tailPVA[1] = new(0, 0);
        minco._tailPVA[2] = new(0, 0);

        var func = ObjectiveFunction.Gradient(x =>
        {
            var tau = x.SubVector(0, path.Length * K);
            var kesi = x.SubVector(path.Length * K, x.Count - path.Length * K);
            minco.Generate(tau, kesi);
            return (minco.GetJ(), minco.GetGradient(kesi, x.Clone()));
        });

        // 设置 L-BFGS 参数
        var minimizer = new LimitedMemoryBfgsMinimizer(1e-6, 1e-6, 1e-6, 50);
        var rst = minimizer.FindMinimum(func, MathNet.Numerics.LinearAlgebra.Vector<double>.Build.Dense((path.Length - 1) + (K - 1) * 2 * path.Length + path.Length * K));
        var x = rst.MinimizingPoint;
        var tau = x.SubVector(0, path.Length * K);
        var kesi = x.SubVector(path.Length * K, x.Count - path.Length * K);
        minco.Generate(tau, kesi);
        var ros = TlarcRosBridge.Domain.RosBridge.Build("MincoTest");
        ros.Publish<IPath2D, TlarcRosBridge.Infrastructure.Messages.Nav.Path>("/path", "/path"
            , TlarcRosBridge.Infrastructure.DataProcess.Publisher.PublishPath);
        ros.Publish<IPath2D, TlarcRosBridge.Infrastructure.Messages.Nav.Path>("/velocity", "/velocity"
            , TlarcRosBridge.Infrastructure.DataProcess.Publisher.PublishPath);
        ros.Publish<IPath2D, TlarcRosBridge.Infrastructure.Messages.Nav.Path>("/traj", "/traj"
        , TlarcRosBridge.Infrastructure.DataProcess.Publisher.PublishPath);
        var traj = minco.GetPositions(0, minco.TotalSecond / 100, 101);
        EventBus<IPath2D>.Instance.Publish("/path", new PathDecorator<IEnumerable<Vector2>>(from p in path select p.Origin));
        EventBus<IPath2D>.Instance.Publish("/traj", new PathDecorator<IEnumerable<Vector2>>(minco.GetPositions(0, minco.TotalSecond / 100, 101)));
        float t = 0;
        EventBus<IPath2D>.Instance.Publish("/velocity", new PathDecorator<IEnumerable<Vector2>>(from p in minco.GetVelocitys(0, minco.TotalSecond / 100, 101) select new Vector2(t += 0.1f, p.Length())));
        Console.WriteLine(minco.TotalSecond);
    }
}
