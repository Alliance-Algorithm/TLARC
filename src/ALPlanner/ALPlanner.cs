using ALPlanner.Collider;
using ALPlanner.Interfaces;
using ALPlanner.TrajectoryOptimizer;
using Maps;
using TlarcKernel;

namespace ALPlanner;

class ALPlanner : Component
{
  [ComponentReferenceFiled]
  ICollider sentry;

  [ComponentReferenceFiled]
  IPositionDecider target;

  [ComponentReferenceFiled]
  IGridMap gridMap;
  SafeCorridor safeCorridor;
  PathPlanner.PathPlanner pathPlanner;
  TrajectoryOptimizer.TrajectoryOptimizer trajectoryOptimizer;

  IO.ROS2Msgs.Nav.Path debugPath1;
  IO.ROS2Msgs.Nav.Path debugPath2;
  IO.ROS2Msgs.Std.Bool reload;
  IO.ROS2Msgs.Std.Bool followMode;
  bool reload_ = false;
  Vector3d[] trajectory = [];
  private Vector3d lastTarget;
  private Vector3d plannerTarget;


  public override void Start()
  {
    debugPath1 = new(IOManager);
    debugPath2 = new(IOManager);
    reload = new(IOManager);
    followMode = new(IOManager);
    debugPath1.RegistryPublisher("debug/path");
    debugPath2.RegistryPublisher("debug/trajectory");
    reload.Subscript("/alplanner/reload", x => reload_ = true);
    followMode.RegistryPublisher("/alplanner/chassis_follow_velocity");
  }

  Dictionary<string, float> timers = [];

  public override void Update()
  {

    var checkTargetChange = new BehaviourTreeCondition(() => reload_ || lastTarget != target.TargetPosition);
    var rePlan = new BehaviourTreeAction(() =>
    {
      var collections = pathPlanner.Search(sentry.Position, target.TargetPosition, sentry.Velocity);
      trajectoryOptimizer.CalcTrajectory(collections);
      trajectory = [.. trajectoryOptimizer.TrajectoryPoints(0, trajectoryOptimizer.MaxTime, 50)];
      return DecisionMaker.ActionState.Success;
    });
    var checkPassTunnel = new BehaviourTreeCondition(() => safeCorridor.Any(x => Math.Min(Math.Pow(x.MaxX - x.MinX, 2), Math.Pow(x.MaxY - x.MinY, 2)) < 0.4f));
    var setFollower = new BehaviourTreeAction(() => { followMode.Publish(true); return DecisionMaker.ActionState.Success; });
    var setFree = new BehaviourTreeAction(() => { followMode.Publish(false); return DecisionMaker.ActionState.Success; });
  }
}
