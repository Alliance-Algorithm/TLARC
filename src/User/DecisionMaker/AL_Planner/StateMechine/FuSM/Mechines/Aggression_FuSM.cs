using System.Numerics;
using AllianceDM.PreInfo;
using AllianceDM.StdComponent;

namespace AllianceDM.ALPlanner;

class Aggression_FuSM : Component, IStateMachine
{
    public bool FirePermit => current_.FirePermit;

    public bool[] LockPermit => current_.LockPermit;

    public Vector2 GimbalAngle => current_.GimbalAngle;

    public Vector2 TargetPosition { get; private set; }

    HeroAgent heroAgent;
    EngineerAgent engineerAgent;
    DecisionMakingInfo decisionMakingInfo;
    UnitInfo unitInfo;
    Transform2D sentry;

    IStateObject current_;
    EngineerTracker engineerTracker_;
    ReconnaissanceState patrol_;
    HeroTracker heroTracker_;
    private long currentTime_;

    public IStateObject AnyState()
    {
        throw new NotImplementedException();
    }

    public override void Start()
    {
        engineerTracker_ = new()
        {
            HeroAgent = heroAgent,
            EngineerAgent = engineerAgent,
            DecisionMakingInfo = decisionMakingInfo,
            UnitInfo = unitInfo
        };
        heroTracker_ = new()
        {
            HeroAgent = heroAgent,
            EngineerAgent = engineerAgent,
            DecisionMakingInfo = decisionMakingInfo,
            UnitInfo = unitInfo
        };
        patrol_ = new()
        {
            HeroAgent = heroAgent,
            EngineerAgent = engineerAgent,
            DecisionMakingInfo = decisionMakingInfo,
            UnitInfo = unitInfo
        };

        engineerTracker_.Patrol = patrol_;
        engineerTracker_.HeroTracker = heroTracker_;
        heroTracker_.EngineerTracker = engineerTracker_;
        heroTracker_.Patrol = patrol_;
        patrol_.HeroTracker = heroTracker_;
        patrol_.EngineerTracker = engineerTracker_;

        current_ = engineerTracker_;
    }

    public override void Update()
    {
        AnyState();
        if (current_.Update(ref current_, currentTime_))
            currentTime_ = DateTime.Now.Ticks;

        if (current_ != patrol_ && (current_.TargetPosition - sentry.position).Length() < 1.5f)
            TargetPosition = sentry.position;
        else
            TargetPosition = current_.TargetPosition;
    }
}