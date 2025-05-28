using ALPlanner.Collider;
using ALPlanner.Interfaces;
using ALPlanner.PathPlanner.PathSearcher;
using DecisionMaker;
using DecisionMaker.Information;
using Maps;
using TrajectoryTracer;

namespace ALPlanner;

class ALPlanner : Component
{
  [ComponentReferenceFiled]
  ICollider sentryWithCollider;
  Transform sentry;

  [ComponentReferenceFiled]
  IPositionDecider target;
  [ComponentReferenceFiled]
  IMap map;
  SafeCorridor safeCorridor;
  PathPlanner.PathPlanner pathPlanner;
  DeTrouble deTrouble;
  TrajectoryOptimizer.TrajectoryOptimizer trajectoryOptimizer;
  DecisionMakingInfo info;

  IO.ROS2Msgs.Nav.Path debugPath;
  IO.ROS2Msgs.Std.Bool reload;
  IO.ROS2Msgs.Std.Bool followMode;
  bool reload_ = false;
  Vector3d[] trajectory = [];
  private Vector3d lastTarget;
  public Vector3d Target;
  DateTime escapeTime = DateTime.Now;
  DateTime stopTime = DateTime.Now;
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
      trajectory.Length == 0
      ||
      trajectoryOptimizer.TrajectoryPoints(
        trajectoryOptimizer.constructTimeToNowInSeconds,
        trajectoryOptimizer.constructTimeToNowInSeconds + 0.1,
        0.1)
      .All(x => (sentryWithCollider.Position - x).Length > 1)
      ||
      trajectoryOptimizer.TrajectoryPoints(
        trajectoryOptimizer.constructTimeToNowInSeconds,
        Math.Min(trajectoryOptimizer.constructTimeToNowInSeconds + 3, trajectoryOptimizer.MaxTime),
        0.1)
      .Any(x => !map.CheckAccessibility(x))
      );
    var plan = new BehaviourTreeAction(() =>
    {
      var collections = pathPlanner.Search(sentryWithCollider.Position,
       deTrouble.Search(target.TargetPosition, target.TargetPosition).PositionInWorld, sentryWithCollider.Velocity);
      trajectoryOptimizer.CalcTrajectory(collections);
      trajectory = [.. trajectoryOptimizer.TrajectoryPoints(0, trajectoryOptimizer.MaxTime, trajectoryOptimizer.MaxTime / 30)];
      reload_ = false;
      return DecisionMaker.ActionState.Success;
    });
    var stop = new BehaviourTreeAction(() =>
    {
      var target = deTrouble.Search(sentryWithCollider.Position, sentryWithCollider.Position);
      trajectoryOptimizer.CalcTrajectory(target.PositionInWorld);
      reload_ = true;
      stopTime = DateTime.Now + TimeSpan.FromSeconds(0.5);
      return DecisionMaker.ActionState.Success;
    });
    var escape = new BehaviourTreeAction(() =>
    {
      var target = deTrouble.Search(sentryWithCollider.Position, sentryWithCollider.Position);
      trajectoryOptimizer.CalcTrajectory(target.PositionInWorld);
      reload_ = true;
      escapeTime = DateTime.Now + TimeSpan.FromSeconds(0.5);
      return DecisionMaker.ActionState.Success;
    });
    var checkInCollision = new BehaviourTreeCondition(() => !map.CheckAccessibility(sentry.Position));
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
    followPlanControl.AddChildren([checkCurrentPosition, stop]);
    var extricationMode = new BehaviourTreeSequence();
    extricationMode.AddChildren([checkInCollision, escape]);
    var planControl = new BehaviourTreeFallback();
    planControl.AddChildren([firstPlanControl, followPlanControl, extricationMode]);
    var root = new BehaviourTreeParallel();
    root.AddChildren([planControl, followModeControl]);
    _root = root;
  }

  Dictionary<string, float> timers = [];

  public override void Update()
  {
    // TlarcSystem.LogInfo($"target : {target.TargetPosition.x},{target.TargetPosition.y}");
    // TlarcSystem.LogInfo($"sentry: {sentry.Position.x},{sentry.Position.y}");

    if (!info.TestMode && info.GameStage != GameStage.STARTED)
    {
      trajectoryOptimizer.CalcTrajectory(sentry.Position);
      reload_ = true;
    }
    else if ((escapeTime - DateTime.Now).TotalSeconds <= 0 && (stopTime - DateTime.Now).TotalSeconds > 0)
    {
      trajectoryOptimizer.CalcTrajectory(sentry.Position);
      reload_ = true;
    }
    else if ((escapeTime - DateTime.Now).TotalSeconds <= 0)
      _root.Action();

    debugPath.Publish(trajectory);
    lastTarget = target.TargetPosition;
    Target = lastTarget;
  }
}
