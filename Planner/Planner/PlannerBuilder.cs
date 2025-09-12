using System.Numerics;
using ALPlanner.Domain.Trajectorys;
using ALPlanner.Infrastructure.Optimizer;
using Kernel.Core.EventBus;
using Kernel.DataInterfaces;
using Kernel.DataInterfaces.Constraints;
using Kernel.DataInterfaces.Navigation;

namespace Planner;

public class PlannerBuilder : IHeader
{
    class PathDecorator<T>(T path) : IPath2D, IHeader where T : IEnumerable<Vector2>
    {
        public IHeader Header => this;

        public int Length => path.Count();

        public string Identifier => "Test";

        public IEnumerable<Vector2> GetPoints() => path;
    }

    int K = 3;

    public string Identifier { get; private set; } = "map_link";

    void ProcessPath(ISafeCorridor2DData<Circle2D> path)
    {
        Minco minco = new(path.Length * K, path.Corridors);
        EventBus<ITrajectory2D>.Instance.Publish("/tlarc/trajectorys", new MincoTrajectory(minco) { Header = this });

    }

    public PlannerBuilder BuildALPlanner()
    {
        EventBus<ISafeCorridor2DData<Circle2D>>.Instance.Subscribe("", path =>
        {
        });



        return this;
    }
}