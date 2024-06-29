using System.Numerics;
using AllianceDM.PreInfo;
using Rosidl.Messages.Builtin;

namespace AllianceDM.ALPlanner;

class ADS_FSM : Component, IStateMechine
{
    // Params 
    public float minEquivalentHpLimitToReturn;
    public float minOutpostHpLimitToReturn;

    // Outputs
    public bool FirePermit => current_.FirePermit;
    public bool[] LockPermit => current_.LockPermit;
    public Vector2 GimbalAngle => current_.GimbalAngle;
    public Vector2 TargetPosition => current_.TargetPosition;

    // Inputs
    private DecisionMakingInfo info;
    private Aggression_FuSM aggression;
    private Defensive_FuSM defensive;


    // Locals
    private AggressionState aggressionState_;
    private DefensiveState defensiveState_;
    private SupplyState supplyState_;
    private IStateObject current_;
    private float currentTime_ = 0;

    public IStateObject AnyState()
    {
        throw new NotImplementedException();
    }

    public override void Start()
    {
        aggressionState_ =
            new AggressionState(minOutpostHpLimitToReturn)
            {
                Info = info,
                AggressionMechine = aggression
            };
        defensiveState_ = new DefensiveState(minEquivalentHpLimitToReturn)
        {
            Info = info,
            DefinsiveMechine = defensive,
        };
        supplyState_ = new SupplyState(minOutpostHpLimitToReturn)
        {
            Info = info
        }
      ;

        aggressionState_.DefensiveState = defensiveState_;
        aggressionState_.SupplyState = supplyState_;

        defensiveState_.SupplyState = supplyState_;

        supplyState_.AggressionState = aggressionState_;
        supplyState_.DefinsiveState = defensiveState_;

        // Entry -> Aggression
        current_ = aggressionState_;
    }

    public override void Update()
    {
        if (current_.Update(ref current_, (DateTime.UtcNow.Ticks - currentTime_) / 10000000.0f))
        {
            currentTime_ = DateTime.UtcNow.Ticks;
        }
        AnyState();
    }
}