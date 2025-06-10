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

  int inTrouble = 0;
  bool lastChassisOutput = false;
  string lastState = "";


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
      BenchMarkBegin();
      TlarcSystem.LogInfo($"Plan Begin");
      var collections = pathPlanner.Search(sentryWithCollider.Position,
       deTrouble.Search(target.TargetPosition, target.TargetPosition).PositionInWorld, sentryWithCollider.Velocity);
      TlarcSystem.LogInfo($"planner: {BenchMarkStep()}");
      trajectoryOptimizer.CalcTrajectory(collections);
      trajectory = [.. trajectoryOptimizer.TrajectoryPoints(0, trajectoryOptimizer.MaxTime, trajectoryOptimizer.MaxTime / 100)];
      TlarcSystem.LogInfo($"optimizer: collections: {collections.Length} : in {BenchMarkStep()} ms");
      reload_ = false;
      inTrouble = 0;
      lastState = "plan";
      return DecisionMaker.ActionState.Success;
    });
    var stop = new BehaviourTreeAction(() =>
    {
      TlarcSystem.LogInfo("In trouble");
      var target = deTrouble.Search(sentryWithCollider.Position, sentryWithCollider.Position);
      var collection = new TrajectoryOptimizer.ConstraintCollection();
      collection.XBegin = new TrajectoryOptimizer.Constraint(0, Matrix.Identity(2), sentry.Position.x, sentry.Position.x);
      collection.YBegin = new TrajectoryOptimizer.Constraint(0, Matrix.Identity(2), sentry.Position.y, sentry.Position.y);
      collection.XBegin.next = new TrajectoryOptimizer.Constraint(1, Matrix.Identity(2), target.PositionInWorld.x, target.PositionInWorld.x);
      collection.YBegin.next = new TrajectoryOptimizer.Constraint(1, Matrix.Identity(2), target.PositionInWorld.y, target.PositionInWorld.y);
      trajectoryOptimizer.CalcTrajectory(collection);
      reload_ = true;
      stopTime = DateTime.Now + TimeSpan.FromSeconds(0.5);
      lastState = "stop";
      return DecisionMaker.ActionState.Success;
    });
    var escape = new BehaviourTreeAction(() =>
    {
      TlarcSystem.LogInfo("In trouble");
      var target = deTrouble.Search(sentryWithCollider.Position, sentryWithCollider.Position);
      var collection = new TrajectoryOptimizer.ConstraintCollection();
      collection.Length = 2;
      collection.TimeStep = 0.5;
      collection.XBegin = new TrajectoryOptimizer.Constraint(0, Matrix.Identity(2), sentry.Position.x, sentry.Position.x);
      collection.YBegin = new TrajectoryOptimizer.Constraint(0, Matrix.Identity(2), sentry.Position.y, sentry.Position.y);
      collection.XBegin.next = new TrajectoryOptimizer.Constraint(1, Matrix.Identity(2), target.PositionInWorld.x, target.PositionInWorld.x);
      collection.YBegin.next = new TrajectoryOptimizer.Constraint(1, Matrix.Identity(2), target.PositionInWorld.y, target.PositionInWorld.y);
      trajectoryOptimizer.CalcTrajectory(collection);
      reload_ = true;
      escapeTime = DateTime.Now + TimeSpan.FromSeconds(1);
      inTrouble = 0;
      lastState = "escape";
      return DecisionMaker.ActionState.Success;
    });
    var updateInTrouble = new BehaviourTreeAction(() =>
    {
      lastState = "updateInTrouble";
      inTrouble++;
      if (inTrouble >= 10) return DecisionMaker.ActionState.Success;
      else return DecisionMaker.ActionState.Failure;
    });
    var checkChassis = new BehaviourTreeCondition(() => info.chassisOutput && !lastChassisOutput);
    var checkInCollider = new BehaviourTreeCondition(() => !map.CheckAccessibility(sentry.Position));
    var checkPassTunnel = new BehaviourTreeCondition(() => safeCorridor.Any(x => Math.Min(Math.Pow(x.MaxX - x.MinX, 2), Math.Pow(x.MaxY - x.MinY, 2)) < 0.4f));
    var setFollower = new BehaviourTreeAction(() => { followMode.Publish(true); return DecisionMaker.ActionState.Success; });
    var setFree = new BehaviourTreeAction(() => { followMode.Publish(false); return DecisionMaker.ActionState.Success; });
    var passTunnel = new BehaviourTreeSequence();
    var checkEscape = new BehaviourTreeCondition(() => escapeTime >= DateTime.Now);
    var checkNoEscaping = new BehaviourTreeCondition(() => escapeTime < DateTime.Now);
    passTunnel.AddChildren([checkPassTunnel, setFollower]);
    var followModeControl = new BehaviourTreeFallback();
    followModeControl.AddChildren([passTunnel, setFree]);
    var firstPlanControl = new BehaviourTreeSequence();
    firstPlanControl.AddChildren([checkNoEscaping, checkTargetChange, plan]);
    var powerOn = new BehaviourTreeSequence();
    powerOn.AddChildren([checkChassis, plan]);
    var followPlanControl = new BehaviourTreeSequence();
    followPlanControl.AddChildren([checkNoEscaping, checkCurrentPosition, updateInTrouble,stop]);
    var escapeControl = new BehaviourTreeSequence();
    escapeControl.AddChildren([checkInCollider, checkNoEscaping, updateInTrouble, escape]);
    var planControl = new BehaviourTreeFallback();
    planControl.AddChildren([escapeControl,firstPlanControl, powerOn, followPlanControl]);
    var root = new BehaviourTreeParallel();
    root.AddChildren([planControl]);
    _root = root;
  }

  Dictionary<string, float> timers = [];

  public override void Update()
  {
    if (!info.chassisOutput && (info.GameStage == GameStage.COUNTDOWN || !info.TestMode))
    {
      trajectoryOptimizer.CalcTrajectory(sentry.Position);
      reload_ = true;
      TlarcSystem.LogInfo("chassis power off");
    }
    else if (stopTime > DateTime.Now || escapeTime > DateTime.Now) { }
    else
      _root.Action();

    debugPath.Publish(trajectory);
    lastTarget = target.TargetPosition;
    Target = lastTarget;
    lastChassisOutput = info.chassisOutput;

    // TlarcSystem.LogInfo(lastState);
  }
}
