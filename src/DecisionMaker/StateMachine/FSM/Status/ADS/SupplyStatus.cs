
using DecisionMaker.Information;
namespace DecisionMaker.StateMachine.FSM.Status;

class SupplyState(float minOutpostHpLimitToReturn) : IStateObject
{
    public bool FirePermit => false;
    public bool[] LockPermit => [false, false, false, false, false, false, false, false];
    public Vector2d GimbalAngle => new(MathF.Tau, MathF.PI);
    public Vector2d TargetPosition => new(-11.94f, -6.74f);


    public IStateObject DefensiveState { get; set; }
    public IStateObject AggressionState { get; set; }
    float MinOutpostHpLimitToReturn { get; } = minOutpostHpLimitToReturn;
    required public DecisionMakingInfo Info { get; init; }

    public bool Update(ref IStateObject state, float timeCoefficient)
    {
        if (Info.BaseArmorOpeningCountdown <= 15)
        {
            state = Info.FriendOutPostHp >= MinOutpostHpLimitToReturn
                    ? AggressionState : DefensiveState;
            return true;
        }
        // this -> LastState
        if (Info.SupplyRFID)
            if (Info.SentryHp >= DecisionMakingInfo.SentryHPLimit - 20)
            {
                state = Info.FriendOutPostHp >= MinOutpostHpLimitToReturn
                ? AggressionState : DefensiveState;
                return true;
            }

        return false;
    }
}