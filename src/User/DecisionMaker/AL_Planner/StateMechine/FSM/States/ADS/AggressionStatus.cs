using System.Numerics;
using AllianceDM.PreInfo;

namespace AllianceDM.ALPlanner;

class AggressionState(float minOutpostHpLimitToReturn) : IStateObject
{
    public bool FirePermit => AggressionMechine.FirePermit;
    public bool[] LockPermit => AggressionMechine.LockPermit;
    public Vector2 GimbalAngle => AggressionMechine.GimbalAngle;
    public Vector2 TargetPosition => AggressionMechine.TargetPosition;
    public required IStateMechine AggressionMechine { get; init; }
    required public DecisionMakingInfo Info { get; init; }
    public IStateObject DefensiveState { get; set; }
    public IStateObject SupplyState { get; set; }
    float MinOutpostHpLimitToReturn { get; } = minOutpostHpLimitToReturn;


    public bool Update(ref IStateObject state, float timeCoefficient = float.NaN)
    {
        // this -> definsive
        if (Info.FriendOutPostHp <= MinOutpostHpLimitToReturn)
        {
            state = DefensiveState;
            return true;
        }

        // this -> supply
        if (Info.BulletCount == 0)
            if (Info.SentryHp <= DecisionMakingInfo.SentinelHPLimit)
            {
                return false;
            }
            else
            {
                state = SupplyState;
                return true;
            }

        return false;
    }
}