using ALPlanner.PathPlanner.PathSearcher;
using ALPlanner.PathPlanner.Sampler;
using Maps;

namespace ALPlanner.PathPlanner;

class PathPlanner : Component
{
  const double TimeStep = 0.1;
  [ComponentReferenceFiled]
  private IPathSearcher NoTimePathSearcher;
  [ComponentReferenceFiled]
  private IPathSearcher<SafeCorridor> TimeConstraintsPathSearcher;

  private SafeCorridor safeCorridor;

  [ComponentReferenceFiled]
  private ISampler sampler;

  public TrajectoryOptimizer.ConstraintCollection Search(Vector3d origin, Vector3d target, Vector3d? speed = null)
  {

    var points = sampler.Sample(NoTimePathSearcher.Search(origin, target, speed));
    safeCorridor.Generate(points);
    points = sampler.Sample(TimeConstraintsPathSearcher.Search(origin, target, safeCorridor, speed));
    safeCorridor.Generate(points);
    var collection = safeCorridor.GenerateConstraintCollection();
    collection.TimeStep = TimeStep;
    return collection;
  }

}
