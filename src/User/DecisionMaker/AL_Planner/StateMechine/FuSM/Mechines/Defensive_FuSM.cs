using System.Numerics;
using AllianceDM.PreInfo;
using AllianceDM.StdComponent;

namespace AllianceDM.ALPlanner;

class Defensive_FuSM : Component, IStateMachine
{
    public float hideTime = 10;
    public bool FirePermit => throw new NotImplementedException();

    public bool[] LockPermit => throw new NotImplementedException();

    public Vector2 GimbalAngle => throw new NotImplementedException();

    public Vector2 TargetPosition => throw new NotImplementedException();

    public IStateObject[] AllState => throw new NotImplementedException();

    HeroAgent heroAgent;
    UVAAgent UVAAgent;
    JumperAgent jumperAgent;
    DecisionMakingInfo decisionMakingInfo;
    UnitInfo unitInfo;
    Transform2D sentry;

    IStateObject current_;
    GreatWallWatcher greatWallWatcher_;
    Hider hider_;
    Portal portal_;
    private long currentTime_;

    public IStateObject AnyState()
    {
        throw new NotImplementedException();
    }
    public override void Start()
    {
        currentTime_ = DateTime.Now.Ticks;

        greatWallWatcher_ = new()
        {
            HeroAgent = heroAgent,
            UVAAgent = UVAAgent,
            JumperAgent = jumperAgent
        };
        hider_ = new(hideTime)
        {
            HeroAgent = heroAgent,
            UVAAgent = UVAAgent,
            JumperAgent = jumperAgent
        };
        portal_ = new()
        {
            HeroAgent = heroAgent,
            UVAAgent = UVAAgent,
            JumperAgent = jumperAgent,
            Sentry = sentry
        };

        greatWallWatcher_.Hider = hider_;
        greatWallWatcher_.Portal = portal_;

        hider_.Portal = portal_;
        hider_.GreatWallWatcher = greatWallWatcher_;

        portal_.GreatWallWatcher = greatWallWatcher_;
        portal_.Hider = hider_;

        // Entry -> Portal
        current_ = portal_;
    }

    public override void Update()
    {
        AnyState();
        if (current_.Update(ref current_, currentTime_))
            currentTime_ = DateTime.Now.Ticks;

    }
}