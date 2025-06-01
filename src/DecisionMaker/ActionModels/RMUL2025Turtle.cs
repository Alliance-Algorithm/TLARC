using ALPlanner.Interfaces;
using DecisionMaker.Information;
using DecisionMaker.StateMachine;

namespace DecisionMaker;

class RMUL2025DecisionMakerTurtle : Component, IPositionDecider
{
  Transform sentry;

  EnemyUnitInfo enemyUnitInfo;
  DecisionMakingInfo decisionMakingInfo;
  StateMachine.StateMachine stateMachine = new();
  BehaviourTreeSequence testModeGotoPatrol;

  IO.ROS2Msgs.Geometry.PoseStampd pose;

  IO.ROS2Msgs.Nav.Path points;



  readonly Vector3d[] SupplyPosition = [new(-11.5, 0.08, 0), new(12.3, 9.24, 0)];
  readonly Vector3d[] EnemyBasePosition = [new(13.9, 2.23, 0), new(-12.8, 6.77, 0)];
  readonly Vector3d[] patrolPoints =
  [
    new(-2.3, 11.23, 0),
    new(-2.3, 11.23, 0),
    new(-2.3, 11.23, 0),

    new(4.3, -2.2, 0),
    new(4.3, -2.2, 0),
    new(4.3, -2.2, 0),
  ];

  public Vector3d TargetPosition { get; private set; }
  public FirePermission FirePermission { get; private set; }

  #region Black Board
  Vector3d bb_patrol_target;
  DateTime bb_arrive_time = DateTime.Now;
  bool bb_arrived = false;
  DateTime bb_hero_tracing_time = DateTime.Now;
  #endregion
  public sealed override void Start()
  {
    pose = new(IOManager);
    pose.RegistryPublisher("/debug/target_position");
    points = new(IOManager);
    points.RegistryPublisher("/debug/important_position");


    #region Black Board
    bb_patrol_target = patrolPoints[0];
    bb_arrive_time = DateTime.Now;
    bb_arrived = false;
    bb_hero_tracing_time = DateTime.Now;
    #endregion

    #region Core Charge Behaviour Tree
    #region     Goto Target
    var gotoPosition = new BehaviourTreeAction(() =>
    {
      if (((sentry.Position - bb_patrol_target).Length > 1.0 && (DateTime.Now - bb_arrive_time).TotalSeconds < 15) && !bb_arrived)
      {
        TargetPosition = bb_patrol_target;
        return ActionState.Running;
      }
      if (((sentry.Position - bb_patrol_target).Length < 1.0 ||  (DateTime.Now - bb_arrive_time).TotalSeconds > 15 )  && !bb_arrived)
      {
        bb_arrive_time = DateTime.Now;
        bb_arrived = true;
        
        return ActionState.Success;
      }
      if(bb_arrived)
        return ActionState.Success;
      return ActionState.Failure;
    });
    var notSearching = new BehaviourTreeCondition(() =>
      (DateTime.Now - bb_arrive_time).TotalSeconds > 5
    );
    var changePosition = new BehaviourTreeAction(() =>
    {
      var k =
        patrolPoints[Random.Shared.Next(0, 2) + (decisionMakingInfo.RobotColor == RobotColor.RED ? 0 : 3)];
      while (k == bb_patrol_target)
        k = patrolPoints[Random.Shared.Next(0, 2) + (decisionMakingInfo.RobotColor == RobotColor.RED ? 0 : 3)];
      bb_patrol_target = k;

      bb_arrived = false;
      Console.WriteLine("Change");
      return ActionState.Success;
    });
    var gotoPatrol = new BehaviourTreeSequence();
    gotoPatrol.AddChildren([ gotoPosition,notSearching, changePosition]);
    testModeGotoPatrol = new BehaviourTreeSequence();
    testModeGotoPatrol.AddChildren([gotoPosition, notSearching, changePosition]);
    #endregion
    //
    #region     Search and Trace Target
   
    #endregion
    //
    var coreChargeBT = new BehaviourTreeParallel();
    coreChargeBT.AddChildren([gotoPatrol]);
    #endregion
    #region  Clash Surge Behaviour Tree
   
    var clashSurgeBT = new BehaviourTreeFallback();
    clashSurgeBT.AddChildren([gotoPatrol]);
    #endregion
    #region  Total State Machine
    var coreCharge = new StateMachineNode(() =>
    {
      coreChargeBT.Action();
    });
    var clashSurge = new StateMachineNode(() =>
    {
      clashSurgeBT.Action();
    });
    var supplySelf = new StateMachineNode(() =>
    {
      TargetPosition = decisionMakingInfo.RobotColor == RobotColor.RED ? SupplyPosition[0] : SupplyPosition[1];
    });
    var headStrike = new StateMachineNode(() =>
    {
      TargetPosition = decisionMakingInfo.RobotColor == RobotColor.RED ? EnemyBasePosition[0] : EnemyBasePosition[1];
      FirePermission = FirePermission.Base;
    });

    var durationFromBegin = DateTime.Now - decisionMakingInfo.GameStartTime;

    stateMachine.FromTo(
      coreCharge,
      () => durationFromBegin.TotalMinutes > 2.5 && durationFromBegin.TotalMinutes <= 5.5,
      clashSurge
    );

    stateMachine.FromTo(
      coreCharge,
      () =>
        durationFromBegin.TotalMinutes > 5.5
        && 5 * (decisionMakingInfo.BulletCount + decisionMakingInfo.BulletSupplyCount)
          > decisionMakingInfo.EnemyBaseHp,
      headStrike
    );

    stateMachine.FromTo(
      clashSurge,
      () =>
        durationFromBegin.TotalMinutes > 5.5
        && 5 * (decisionMakingInfo.BulletCount + decisionMakingInfo.BulletSupplyCount)
          > decisionMakingInfo.EnemyBaseHp,
      headStrike
    );

    stateMachine.FromTo(
      supplySelf,
      () => durationFromBegin.TotalMinutes <= 2.5 && decisionMakingInfo.SentryHp > 350,
      coreCharge
    );

    stateMachine.FromTo(
      supplySelf,
      () =>
        durationFromBegin.TotalMinutes > 2.5
        && durationFromBegin.TotalMinutes <= 5.5
        && decisionMakingInfo.SentryHp > 350,
      clashSurge
    );

    stateMachine.FromTo(
      supplySelf,
      () =>
        durationFromBegin.TotalMinutes > 5.5
        && decisionMakingInfo.SentryHp > 350
        && decisionMakingInfo.BulletSupplyCount == 0,
      headStrike
    );

    stateMachine.FromTo(
      headStrike,
      () =>
        decisionMakingInfo.BulletSupplyCount >= 200
        && decisionMakingInfo.SentryHp < 300
        && sentry.Position.x < 0,
      supplySelf
    );

    stateMachine.AnyTo(() => decisionMakingInfo.SentryHp < 200, supplySelf);
    stateMachine.BeginTo(coreCharge);
    #endregion
  }
  public override void Update()
  {

    if (decisionMakingInfo.TestMode)
      testModeGotoPatrol.Action();
    else
      stateMachine.Step();
    pose.Publish((new System.Numerics.Vector2((float)bb_patrol_target.x, (float)bb_patrol_target.y), 0));
    points.Publish([.. patrolPoints, .. EnemyBasePosition, .. SupplyPosition]);
  }
}
