using ALPlanner.PathPlanner.Nodes;
using ALPlanner.PathPlanner.PathSearcher;
using ALPlanner.PathPlanner.Sampler;
using Maps;

namespace ALPlanner.PathPlanner;

class PathPlanner : Component
{
  public const double TimeStep = 0.04;
  [ComponentReferenceFiled]
  private IPathSearcher NoTimePathSearcher;
  [ComponentReferenceFiled]
  private IPathSearcher<SafeCorridor> TimeConstraintsPathSearcher;

  private SafeCorridor safeCorridor;
  IO.ROS2Msgs.Nav.Path debugPath;
  IO.ROS2Msgs.Nav.Path debugPath1;

  [ComponentReferenceFiled]
  private ISampler sampler;
  Transform sentry;

  Dictionary<string, float> benchMark = [];

  public override void Start()
  {
    debugPath = new(IOManager);
    debugPath.RegistryPublisher("debug/path/a_star");
    debugPath1 = new(IOManager);
    debugPath1.RegistryPublisher("debug/path/kino");
  }

  public TrajectoryOptimizer.ConstraintCollection Search(Vector3d origin, Vector3d target, Vector3d? speed = null)
  {

    BenchMarkBegin();
    var points = sampler.Sample(NoTimePathSearcher.Search(origin + 0.5 * sentry.Velocity, target, speed));
    // Console.WriteLine($"a:{Omni2DConsecNode.Headings} astar,{ BenchMarkStep()}");
    // Omni2DConsecNode.Headings += 1;
    BenchMarkEnd();
    // Console.WriteLine($"1: {BenchMarkStep()}");
    debugPath.Publish(points.Copy());
    safeCorridor.Generate(points, 5);
    // BenchMarkFilled("astar safeCorridor", BenchMarkStep(), ref benchMark);
    // Console.WriteLine($"2: {BenchMarkStep()}");
    // points = sampler.Sample(TimeConstraintsPathSearcher.Search(origin, target, safeCorridor, speed));
    // BenchMarkFilled("kino ", BenchMarkStep(), ref benchMark);
    // debugPath1.Publish(points.Copy());
    // BenchMarkFilled("kino safeCorridor", BenchMarkStep(), ref benchMark);
    // safeCorridor.Generate(points, TimeStep);
    var collection = safeCorridor.GenerateConstraintCollection();
    // BenchMarkFilled("Constraint generate", BenchMarkStep(), ref benchMark);
    collection.TimeStep = TimeStep;
    // TlarcSystem.SetLogTimers("Path Planner", benchMark);
    return collection;
  }

}
