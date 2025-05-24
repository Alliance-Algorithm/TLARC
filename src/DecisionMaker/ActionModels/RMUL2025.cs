using ALPlanner.Interfaces;
using DecisionMaker.Information;
using DecisionMaker.StateMachine;

namespace DecisionMaker;

class RMUL2025DecisionMaker : Component, IPositionDecider
{
  Transform sentry;

  EnemyUnitInfo enemyUnitInfo;
  DecisionMakingInfo decisionMakingInfo;
  StateMachine.StateMachine stateMachine = new();
  readonly Vector3d SupplyPosition = new(-12.5, -5, 0);
  readonly Vector3d EnemyBasePosition = new(12.5, -1.5, 0);
  readonly Vector3d FortressPosition = new(-7.5, 0, 0);
  readonly Vector3d[] patrolPoints =
  [
    new(-2, 2.7, 0),
    new(2, -2.7, 0),
    new(10, 2, 0),
    new(5, -6, 0),
  ];

  public Vector3d TargetPosition { get; private set; }
  public FirePermission FirePermission { get; private set; }

  public sealed override void Start()
  {
    #region Black Board
    Vector3d bb_patrol_target = patrolPoints[0];
    DateTime bb_arrive_time = DateTime.Now;
    bool bb_arrived = false;
    DateTime bb_hero_tracing_time = DateTime.Now;
    #endregion


    #region Core Charge Behaviour Tree
    #region     Goto Target
    var notFindEngineer = new BehaviourTreeCondition(() => !enemyUnitInfo.Found[EnemyUnitInfo.Engineer]);
    var gotoPosition = new BehaviourTreeAction(() =>
    {
      if ((sentry.Position - bb_patrol_target).Length > 0.5)
      {
        TargetPosition = bb_patrol_target;
        return ActionState.Running;
      }
      if ((sentry.Position - bb_patrol_target).Length < 0.5 && !bb_arrived)
      {
        bb_arrive_time = DateTime.Now;
        bb_arrived = true;
      }
      return ActionState.Success;
    });
    var notSearching = new BehaviourTreeCondition(() =>
      (DateTime.Now - bb_arrive_time).TotalSeconds > 10
    );
    var changePosition = new BehaviourTreeAction(() =>
    {
      bb_patrol_target = patrolPoints[Random.Shared.Next(0, 3)];
      bb_arrived = false;
      return ActionState.Success;
    });
    var gotoPatrol = new BehaviourTreeSequence();
    gotoPatrol.AddChildren([notFindEngineer, gotoPosition, notSearching, changePosition]);
    #endregion
    //
    #region     Search and Trace Target
    var findEngineer = new BehaviourTreeCondition(() => enemyUnitInfo.Found[EnemyUnitInfo.Engineer]);
    var tracingEngineer = new BehaviourTreeAction(() =>
    {
      if (enemyUnitInfo.Locked == EnemyUnitInfo.Engineer && (sentry.Position.xy - enemyUnitInfo.Position[EnemyUnitInfo.Engineer]).Length < 1)
        TargetPosition = sentry.Position;
      else
        TargetPosition = new(enemyUnitInfo.Position[EnemyUnitInfo.Engineer].x, enemyUnitInfo.Position[EnemyUnitInfo.Engineer].y, 0);
      return ActionState.Success;
    });
    var searchAndTraceTarget = new BehaviourTreeSequence();
    searchAndTraceTarget.AddChildren([findEngineer, tracingEngineer]);
    #endregion
    //
    var coreChargeBT = new BehaviourTreeParallel();
    coreChargeBT.AddChildren([gotoPatrol, searchAndTraceTarget]);
    #endregion
    #region  Clash Surge Behaviour Tree
    var notFindHero = new BehaviourTreeCondition(() =>
      !enemyUnitInfo.Found[EnemyUnitInfo.Hero] && (DateTime.Now - bb_hero_tracing_time).TotalSeconds >= 10
    );
    var setPosition = new BehaviourTreeAction(() =>
    {
      bb_patrol_target = FortressPosition;
      return ActionState.Success;
    });
    var gotoFortress = new BehaviourTreeSequence();
    gotoFortress.AddChildren([notFindHero, setPosition, gotoPosition]);

    var findHero = new BehaviourTreeCondition(() =>
      enemyUnitInfo.Found[EnemyUnitInfo.Hero] || (DateTime.Now - bb_hero_tracing_time).TotalSeconds < 10
    );
    var lockHero = new BehaviourTreeCondition(() => enemyUnitInfo.Locked == EnemyUnitInfo.Hero);
    var setHeroFindTime = new BehaviourTreeAction(() =>
    {
      if ((DateTime.Now - bb_hero_tracing_time).TotalSeconds > 10)
        bb_hero_tracing_time = DateTime.Now;
      return ActionState.Success;
    });
    var tracingHero = new BehaviourTreeAction(() =>
    {
      if (enemyUnitInfo.Position[EnemyUnitInfo.Hero].x < 0)
      {
        bb_hero_tracing_time = DateTime.Now;
        return ActionState.Failure;
      }
      if (enemyUnitInfo.Locked == EnemyUnitInfo.Hero && (sentry.Position.xy - enemyUnitInfo.Position[EnemyUnitInfo.Hero]).Length < 1)
        TargetPosition = sentry.Position;
      else
        TargetPosition = new(enemyUnitInfo.Position[EnemyUnitInfo.Hero].x, enemyUnitInfo.Position[EnemyUnitInfo.Hero].y, 0);
      return ActionState.Success;
    });
    var inFortress = new BehaviourTreeCondition(() =>
      (sentry.Position - FortressPosition).Length < 0.5
    );
    var lockInFortress = new BehaviourTreeSequence();
    lockInFortress.AddChildren([lockHero, inFortress]);
    var lockingHero = new BehaviourTreeFallback();
    lockingHero.AddChildren([lockInFortress, tracingHero]);
    var searchHero = new BehaviourTreeSequence();
    searchHero.AddChildren([findHero, setHeroFindTime, lockingHero]);
    var clashSurgeBT = new BehaviourTreeFallback();
    clashSurgeBT.AddChildren([lockingHero, gotoFortress]);
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
      TargetPosition = SupplyPosition;
    });
    var headStrike = new StateMachineNode(() =>
    {
      TargetPosition = EnemyBasePosition;
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

    stateMachine.AnyTo(() => decisionMakingInfo.SentryHp < 150, supplySelf);
    stateMachine.BeginTo(coreCharge);
    #endregion
  }
  public override void Update()
  {
    stateMachine.Step();
  }
}
