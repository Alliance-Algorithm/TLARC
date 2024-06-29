using System.Numerics;
using AllianceDM.PreInfo;
using Rosidl.Messages.Builtin;

namespace AllianceDM.ALPlanner;

class ADS_FSM : Component, IStateMachine
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
    private DecisionMakingInfo decisionMakingInfo;
    private UnitInfo unitInfo;
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
        currentTime_ = DateTime.Now.Ticks;

        aggressionState_ =
            new AggressionState(minOutpostHpLimitToReturn)
            {
                DecisionMakingInfo = decisionMakingInfo,
                AggressionMachine = aggression,
                UnitInfo = unitInfo
            };
        defensiveState_ = new DefensiveState(minEquivalentHpLimitToReturn)
        {
            Info = decisionMakingInfo,
            DefensiveMachine = defensive,
        };
        supplyState_ = new SupplyState(minOutpostHpLimitToReturn)
        {
            Info = decisionMakingInfo
        }
      ;

        aggressionState_.DefensiveState = defensiveState_;
        aggressionState_.SupplyState = supplyState_;

        defensiveState_.SupplyState = supplyState_;

        supplyState_.AggressionState = aggressionState_;
        supplyState_.DefensiveState = defensiveState_;

        // Entry -> Aggression
        current_ = aggressionState_;
    }

    public override void Update()
    {
        AnyState();
        if (current_.Update(ref current_, (DateTime.Now.Ticks - currentTime_) / 1e7f))
        {
            currentTime_ = DateTime.Now.Ticks;
        }
    }
}