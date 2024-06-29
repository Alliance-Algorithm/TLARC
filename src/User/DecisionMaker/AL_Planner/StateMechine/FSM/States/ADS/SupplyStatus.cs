using System.Numerics;
using AllianceDM.PreInfo;
using Microsoft.Toolkit.HighPerformance;

namespace AllianceDM.ALPlanner;

class SupplyState(float minOutpostHpLimitToReturn) : IStateObject
{
    public bool FirePermit => false;
    public bool[] LockPermit => [false, false, false, false, false, false, false, false];
    public Vector2 GimbalAngle => new(MathF.Tau, MathF.PI);
    public Vector2 TargetPosition => new(-11.94f, -6.74f);


    public IStateObject DefinsiveState { get; set; }
    public IStateObject AggressionState { get; set; }
    float MinOutpostHpLimitToReturn { get; } = minOutpostHpLimitToReturn;
    required public DecisionMakingInfo Info { get; init; }

    public bool Update(ref IStateObject state, float timeCoefficient)
    {
        // this -> LastState
        if (Info.SupplyRFID)
            if (Info.SentryHp >= DecisionMakingInfo.SentinelHPLimit - 20)
            {
                state = Info.FriendOutPostHp >= MinOutpostHpLimitToReturn
                ? AggressionState : DefinsiveState;
                return true;
            }

        return false;
    }
}