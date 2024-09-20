using DecisionMaker.Information;
namespace DecisionMaker.StateMachine.FSM.Status;

class AggressionState(float minOutpostHpLimitToReturn) : IStateObject
{
    public bool FirePermit => AggressionMachine.FirePermit;
    public bool[] LockPermit => _lockPermit;
    private bool[] _lockPermit;
    public Vector2d GimbalAngle => AggressionMachine.GimbalAngle;
    public Vector2d TargetPosition => AggressionMachine.TargetPosition;
    public required IStateMachine AggressionMachine { get; init; }
    required public DecisionMakingInfo DecisionMakingInfo { get; init; }
    required public EnemyUnitInfo UnitInfo { get; init; }
    public IStateObject DefensiveState { get; set; }
    public IStateObject SupplyState { get; set; }
    float MinOutpostHpLimitToReturn { get; } = minOutpostHpLimitToReturn;


    public bool Update(ref IStateObject state, float timeCoefficient = float.NaN)
    {
        _lockPermit = AggressionMachine.LockPermit;
        _lockPermit[5] = float.IsInfinity(UnitInfo.Hp[5]);
        // this -> defensive
        if (DecisionMakingInfo.FriendOutPostHp <= MinOutpostHpLimitToReturn)
        {
            state = DefensiveState;
            return true;
        }

        // this -> supply
        if (DecisionMakingInfo.BulletCount == 0 ||
        (UnitInfo.EquivalentHp[(int)RobotType.Hero] == float.PositiveInfinity
        && UnitInfo.EquivalentHp[(int)RobotType.Engineer] == float.PositiveInfinity))
            if (DecisionMakingInfo.SentryHp < DecisionMakingInfo.SentryHPLimit || DecisionMakingInfo.BulletSupplyCount == 0)
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