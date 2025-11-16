using System.Numerics;
using ALPlanner.Domain.Trajectorys;
using ALPlanner.Infrastructure.Optimizer;
using ALPlanner.Infrastructure.PathSearcher;
using Kernel.Core.EventBus;
using Kernel.Contract;
using Kernel.Contract.Constraints;
using Kernel.Contract.Navigation;
using Kernel.Contract.Visualization;
using Kernel.Contract.Geometry;
using Kernel.Core.TransformTree;

namespace Planner;

public class PlannerBuilder<MapT> where MapT : IMap2D
{

    AStar? _aStar;
    MapT? _map;
    IObstacle? obstacle;

    public string SdfMapTopicName { get; private set; } = "/tlarc/map";
    public string ObstacleMapTopicName { get; private set; } = "/tlarc/obstacle";
    public float PathSearchIteratorStep { get; private set; } = 0.1f;
    public float MapResolution { get; private set; }
    public int MapWidth { get; private set; }
    public int MapHight { get; private set; }

    public string Identifier { get; set; } = "cost_map_link";

    public Path2D SeachPath(Pose2D from, Pose2D to){ 
            Vector3 fromPos  = Tf.Cast(from.Header.Identifier, Identifier    , new(from.Translation,0)   , from.Header.Timestamp.ToStamp);
            Vector3 toPos    = Tf.Cast(to.Header.Identifier,   Identifier      , new(to.Translation,0)     , from.Header.Timestamp.ToStamp);
            return new() { 
                Points = _map is not null ? _aStar!.Search(new(fromPos.X,fromPos.Y), new(toPos.X,toPos.Y), _map!) : [],
                Header = new Header{Identifier = Identifier}};
        }
    public SafeCorridor2DData<Circle>
        SearchCircleSafeCorridor
        (Path2D path)
        => ALPlanner.Infrastructure.SafeCorridorConstruct.GaussianSample.RadiusWithDistance(path, obstacle!);

    public SafeCorridor2DData<AABB2D>
        SearchAABBSafeCorridor
        (Path2D path)
        => Tlarc.Map.SafeCorridor.RectangleIncrement.AABB(path, _map!,MapResolution);

    public static ITrajectory2D? OptimizePath<T>
        (SafeCorridor2DData<T> corridor,
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


    public PlannerBuilder<MapT> BuildALPlanner()
    {
        _aStar = new(MapWidth, MapHight, PathSearchIteratorStep, MapResolution);
        // EventBus<MapT>
        //     .Instance.Subscribe(
        //         SdfMapTopicName,
        //         map => _gridmap = map);

        return this;
    }
    public PlannerBuilder<MapT> SetGridMapEventName(string name)
    {
        SdfMapTopicName = name;
        return this;
    }
    public PlannerBuilder<MapT> SetObstacleMapEventName(string name)
    {
        ObstacleMapTopicName = name;
        return this;
    }
    public PlannerBuilder<MapT> SetMapWidth(int width)
    {
        MapWidth = width;
        return this;
    }
    public PlannerBuilder<MapT> SetMapHeight(int height)
    {
        MapHight = height;
        return this;
    }
    public PlannerBuilder<MapT> SetMapResolution(float resolution)
    {
        MapResolution = resolution;
        return this;
    }
    public PlannerBuilder<MapT> SetPathSearchIteratorStep(float step)
    {
        PathSearchIteratorStep = step;
        return this;
    }
    public PlannerBuilder<MapT> SetMap(MapT map) 
    { 
        _map = map;
        return this;
    }
}