using System.Numerics;
using AllianceDM.PreInfo;
using AllianceDM.StdComponent;

namespace AllianceDM.ALPlanner;

class Defensive_FuSM : Component, IStateMachine
{
    public float hideTime = 10;
    public bool FirePermit => current_.FirePermit;

    public bool[] LockPermit => current_.LockPermit;

    public Vector2 GimbalAngle => current_.GimbalAngle;

    public Vector2 TargetPosition => current_.TargetPosition;


    HeroAgent heroAgent;
    UVAAgent UVAAgent;
    JumperAgent jumperAgent;
    Transform2D sentry;
    DecisionMakingInfo decisionMakingInfo;

    IStateObject current_;
    GreatWallWatcher greatWallWatcher_;
    Hider hider_;
    Portal portal_;
    private long currentTime_;

    public IStateObject AnyState()
    {
        return current_;
    }
    public override void Start()
    {
        currentTime_ = DateTime.Now.Ticks;

        greatWallWatcher_ = new()
        {
            HeroAgent = heroAgent,
            UVAAgent = UVAAgent,
            JumperAgent = jumperAgent,
            Info = decisionMakingInfo
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