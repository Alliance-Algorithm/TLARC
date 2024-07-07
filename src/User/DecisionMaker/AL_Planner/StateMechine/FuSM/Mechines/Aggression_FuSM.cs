using System.Numerics;
using AllianceDM.PreInfo;
using AllianceDM.StdComponent;

namespace AllianceDM.ALPlanner;

class Aggression_FuSM : Component, IStateMachine
{
    public bool FirePermit => current_.FirePermit;

    public bool[] LockPermit => current_.LockPermit;

    public Vector2 GimbalAngle => current_.GimbalAngle;

    public Vector2 TargetPosition => current_.TargetPosition;

    HeroAgent heroAgent;
    EngineerAgent engineerAgent;
    DecisionMakingInfo decisionMakingInfo;
    EnemyUnitInfo unitInfo;
    Transform2D sentry;

    IStateObject current_;
    EngineerTracker engineerTracker_;
    ReconnaissanceState reconnaissanceState_;
    HeroTracker heroTracker_;
    private long currentTime_;

    public IStateObject AnyState()
    {
        return current_;
    }

    public override void Start()
    {
        currentTime_ = DateTime.Now.Ticks;

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
        reconnaissanceState_ = new()
        {
            HeroAgent = heroAgent,
            EngineerAgent = engineerAgent,
            DecisionMakingInfo = decisionMakingInfo,
            UnitInfo = unitInfo
        };

        engineerTracker_.Patrol = reconnaissanceState_;
        engineerTracker_.HeroTracker = heroTracker_;
        heroTracker_.EngineerTracker = engineerTracker_;
        heroTracker_.Patrol = reconnaissanceState_;
        reconnaissanceState_.HeroTracker = heroTracker_;
        reconnaissanceState_.EngineerTracker = engineerTracker_;

        // Entry -> EngineerTracker
        current_ = engineerTracker_;
    }

    public override void Update()
    {
        AnyState();
        if (current_.Update(ref current_, currentTime_))
            currentTime_ = DateTime.Now.Ticks;
    }
}