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
  IMap map;
  SafeCorridor safeCorridor;
  PathPlanner.PathPlanner pathPlanner;
  TrajectoryOptimizer.TrajectoryOptimizer trajectoryOptimizer;

  IO.ROS2Msgs.Nav.Path debugPath;
  IO.ROS2Msgs.Std.Bool reload;
  IO.ROS2Msgs.Std.Bool followMode;
  bool reload_ = false;
  Vector3d[] trajectory = [];
  private Vector3d lastTarget;

  BehaviourTree _root;

  public override void Start()
  {
    debugPath = new(IOManager);
    reload = new(IOManager);
    followMode = new(IOManager);
    debugPath.RegistryPublisher("debug/trajectory");
    reload.Subscript("/alplanner/reload", x => reload_ = true);
    followMode.RegistryPublisher("/alplanner/chassis_follow_velocity");

    // Behaviour
    var checkTargetChange = new BehaviourTreeCondition(() => reload_ || lastTarget != target.TargetPosition);
    var checkCurrentPosition = new BehaviourTreeCondition(() =>
      trajectory.Length == 0 ||
      trajectoryOptimizer.TrajectoryPoints(trajectoryOptimizer.constructTimeToNowInSeconds, trajectoryOptimizer.constructTimeToNowInSeconds + 0.1, 0.1)
        .All(x => (sentry.Position - x).Length > 1) || trajectory.Any(x => !map.CheckAccessibility(x,1))
      );
    var plan = new BehaviourTreeAction(() =>
    {
      var collections = pathPlanner.Search(sentry.Position, target.TargetPosition, sentry.Velocity);
      trajectoryOptimizer.CalcTrajectory(collections);
      trajectory = [.. trajectoryOptimizer.TrajectoryPoints(0, trajectoryOptimizer.MaxTime, 0.1)];
      return DecisionMaker.ActionState.Success;
    });
    var checkPassTunnel = new BehaviourTreeCondition(() => safeCorridor.Any(x => Math.Min(Math.Pow(x.MaxX - x.MinX, 2), Math.Pow(x.MaxY - x.MinY, 2)) < 0.4f));
    var setFollower = new BehaviourTreeAction(() => { followMode.Publish(true); return DecisionMaker.ActionState.Success; });
    var setFree = new BehaviourTreeAction(() => { followMode.Publish(false); return DecisionMaker.ActionState.Success; });
    var passTunnel = new BehaviourTreeSequence();
    passTunnel.AddChildren([checkPassTunnel, setFollower]);
    var followModeControl = new BehaviourTreeFallback();
    followModeControl.AddChildren([passTunnel, setFree]);
    var firstPlanControl = new BehaviourTreeSequence();
    firstPlanControl.AddChildren([checkTargetChange, plan]);
    var followPlanControl = new BehaviourTreeSequence();
    followPlanControl.AddChildren([checkCurrentPosition, plan]);
    var planControl = new BehaviourTreeFallback();
    planControl.AddChildren([firstPlanControl, followPlanControl]);
    var root = new BehaviourTreeParallel();
    root.AddChildren([planControl, followModeControl]);
    _root = root;
  }

  Dictionary<string, float> timers = [];

  public override void Update()
  {
    _root.Action();
    debugPath.Publish(trajectory);
    lastTarget = target.TargetPosition;
  }
}
