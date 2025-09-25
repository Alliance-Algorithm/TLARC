using System.Numerics;
using ALPlanner.Domain.Trajectorys;
using ALPlanner.Infrastructure.Optimizer;
using ALPlanner.Infrastructure.PathSearcher;
using Kernel.Core.EventBus;
using Kernel.DataInterfaces;
using Kernel.DataInterfaces.Constraints;
using Kernel.DataInterfaces.Navigation;
using Kernel.DataInterfaces.Visualization;

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

    ALPlanner.Infrastructure.PathSearcher.AStar? _aStar;
    ISdf2D? _sdf2d;
    IObstacle? obstacle;

    public string SdfMapTopicName { get; private set; } = "/tlarc/map";
    public string ObstacleMapTopicName { get; private set; } = "/tlarc/obstacle";
    public float PathSearchIteratorStep { get; private set; } = 0.2f;
    public float MapResolution { get; private set; }
    public int MapWidth { get; private set; }
    public int MapHight { get; private set; }

    public string Identifier { get; private set; } = "map_link";

    public
    IPath2D
        SeachPath
        (Vector2 from, Vector2 to)
        => _aStar!.Search(from, to, _sdf2d!);

    public
    ISafeCorridor2DData<ICircle>
        SearchCircleSafeCorridor
        (IPath2D path)
        => ALPlanner.Infrastructure.SafeCorridorConstruct.GaussianSample.RadiusWithDistance(path, obstacle!);

    public
    ISafeCorridor2DData<AABB2D>
        SearchAABBSafeCorridor
        (IPath2D path)
        => ALPlanner.Infrastructure.SafeCorridorConstruct.IncrementalRectangle.AABBGenerate(path, obstacle!);

    public static
    ITrajectory2D?
        OptimizePath<T>
        (ISafeCorridor2DData<T> corridor,
         MincoOptimizer.Status header,
         MincoOptimizer.Status tail)
        where T : IConstraint
        => MincoOptimizer.Optimize(corridor, header, tail);

    // public static
    // ITrajectory2D?
    //     OptimizePath
    //     (ISafeCorridor2DData<AABB2D> corridor,
    //      MincoOptimizer.Status header,
    //      MincoOptimizer.Status tail)
    //     => MincoOptimizer.Optimize(corridor, header, tail);


    public PlannerBuilder BuildALPlanner()
    {
        _aStar = new(MapWidth, MapHight, PathSearchIteratorStep, MapResolution);
        EventBus<ISdf2D>
            .Instance.Subscribe(
                SdfMapTopicName,
                map => _sdf2d = map);

        EventBus<IObstacle>
            .Instance.Subscribe(
                ObstacleMapTopicName,
                map => obstacle = map);

        return this;
    }
    public PlannerBuilder SetSdfMapEventName(string name)
    {
        SdfMapTopicName = name;
        return this;
    }
    public PlannerBuilder SetObstacleMapEventName(string name)
    {
        ObstacleMapTopicName = name;
        return this;
    }
    public PlannerBuilder SetMapWidth(int width)
    {
        MapWidth = width;
        return this;
    }
    public PlannerBuilder SetMapHeight(int height)
    {
        MapHight = height;
        return this;
    }
    public PlannerBuilder SetMapResolution(float resolution)
    {
        MapResolution = resolution;
        return this;
    }
    public PlannerBuilder SetPathSearchIteratorStep(float step)
    {
        PathSearchIteratorStep = step;
        return this;
    }

}