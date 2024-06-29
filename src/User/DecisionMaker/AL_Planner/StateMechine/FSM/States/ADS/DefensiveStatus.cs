using System.Numerics;
using AllianceDM.PreInfo;

namespace AllianceDM.ALPlanner;
class DefensiveState(float minEquivalentHpLimitToReturn) : IStateObject
{
    public bool FirePermit => DefensiveMachine.FirePermit;
    public bool[] LockPermit => DefensiveMachine.LockPermit;
    public Vector2 GimbalAngle => DefensiveMachine.GimbalAngle;
    public Vector2 TargetPosition => DefensiveMachine.TargetPosition;

    float MinEquivalentHpLimitToReturn { get; } = minEquivalentHpLimitToReturn;
    public IStateObject SupplyState { get; set; }
    required public DecisionMakingInfo Info { get; init; }
    required public IStateMachine DefensiveMachine { get; init; }
    public bool Update(ref IStateObject state, float timeCoefficient = float.NaN)
    {
        //this -> Suplly
        float equivalentHp = Info.SentryHp * Info.DefenseBuff;
        if (equivalentHp < MinEquivalentHpLimitToReturn)
        {
            state = SupplyState;
            return true;
        }
        return false;
    }
}