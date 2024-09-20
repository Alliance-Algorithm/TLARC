using System.Numerics;
using DecisionMaker.Information;
using DecisionMaker.Predictor;

namespace DecisionMaker.StateMachine.FuSM.Status;
class ReconnaissanceState : IStateObject
{
    public bool FirePermit { get; private set; }

    public bool[] LockPermit => [true, true, false, false, false, false, false];

    public Vector2d GimbalAngle { get; private set; } = new(0, 0);

    public Vector2d TargetPosition { get; private set; } = new(3.94f, -6.96f);


    public required HeroAgent HeroAgent { get; init; }
    public required EngineerAgent EngineerAgent { get; init; }
    public required DecisionMakingInfo DecisionMakingInfo { get; init; }
    public required EnemyUnitInfo UnitInfo { get; init; }
    float OutpostHp = DecisionMakingInfo.OutpostHPLimit;
    public IStateObject EngineerTracker { get; set; }
    public IStateObject HeroTracker { get; set; }


    Vector2d[] positions_ = [new(3.94f, -6.96f), new(7.87f, 1.74f)];
    private int presetIndex_ = 0;
    private long timeTick_ = DateTime.Now.Ticks;

    public bool Update(ref IStateObject state, float timeCoefficient)
    {
        if (DecisionMakingInfo.SupplyRFID)
        {
            state = EngineerTracker;
            return true;
        }
        if ((DateTime.Now.Ticks - timeTick_) / 1e7f > 10)
        {
            TargetPosition = positions_[presetIndex_];
            timeTick_ = DateTime.Now.Ticks;
            presetIndex_ = (presetIndex_ + 1) % 2;
        }


        LockPermit[(int)RobotType.Hero] = UnitInfo.EquivalentHp[(int)RobotType.Hero] < 1000;
        LockPermit[(int)RobotType.Engineer] = UnitInfo.EquivalentHp[(int)RobotType.Engineer] < 1000;
        FirePermit = (EngineerAgent.Distance < 6) && EngineerAgent.Locked;
        var engineerCoefficient = EngineerAgent.Value;
        var heroCoefficient = HeroAgent.Value + (OutpostHp != DecisionMakingInfo.FriendOutPostHp ? 100000 : 0);
        OutpostHp = DecisionMakingInfo.FriendOutPostHp;
        double patrolCoefficient = (HeroAgent.Found ? 1 : 100) * (EngineerAgent.Found ? 1 : 100);


        var total = heroCoefficient + engineerCoefficient + patrolCoefficient;
        engineerCoefficient /= total;
        heroCoefficient = heroCoefficient / total + engineerCoefficient;


        engineerCoefficient *= Math.Clamp(timeCoefficient, 0, 1);
        heroCoefficient *= Math.Clamp(timeCoefficient, 0, 1);

        var rand = Random.Shared.NextDouble();

        if (rand < engineerCoefficient)
        {
            state = EngineerTracker;
        }
        else if (rand < heroCoefficient)
        {
            state = HeroTracker;
        }
        else
            return false;
        return true;
    }
}