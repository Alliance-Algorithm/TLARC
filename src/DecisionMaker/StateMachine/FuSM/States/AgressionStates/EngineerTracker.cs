using System.Numerics;
using DecisionMaker.Information;
using DecisionMaker.Predictor;

namespace DecisionMaker.StateMachine.FuSM.Status;

class EngineerTracker : IStateObject
{
    public bool FirePermit { get; private set; } = false;

    public bool[] LockPermit { get; private set; } = [true, true, false, false, false, false, false];

    public Vector2d GimbalAngle { get; private set; }

    public Vector2d TargetPosition => EngineerAgent.Position;

    public required HeroAgent HeroAgent { get; init; }
    public required EngineerAgent EngineerAgent { get; init; }
    public required DecisionMakingInfo DecisionMakingInfo { get; init; }
    public required EnemyUnitInfo UnitInfo { get; init; }
    public IStateObject Patrol { get; set; }
    public IStateObject HeroTracker { get; set; }


    float OutpostHp = DecisionMakingInfo.OutpostHPLimit;

    public bool Update(ref IStateObject state, float timeCoefficient)
    {
        LockPermit[(int)RobotType.Hero] = UnitInfo.EquivalentHp[(int)RobotType.Hero] < 100;
        LockPermit[(int)RobotType.Engineer] = UnitInfo.EquivalentHp[(int)RobotType.Engineer] < 1000;
        GimbalAngle = new(EngineerAgent.Angle - MathF.PI / 4, EngineerAgent.Angle + MathF.PI / 4);
        FirePermit = (EngineerAgent.Distance < 6) && EngineerAgent.Locked;
        var engineerCoefficient = EngineerAgent.Value;
        var heroCoefficient = HeroAgent.Value + (OutpostHp != DecisionMakingInfo.FriendOutPostHp ? 10000 : 0);
        double patrolCoefficient = (HeroAgent.Found ? 1 : 10) * (EngineerAgent.Found ? 1 : 10);


        var total = heroCoefficient + engineerCoefficient + patrolCoefficient;
        patrolCoefficient /= total;
        heroCoefficient = heroCoefficient / total + patrolCoefficient;

        patrolCoefficient *= Math.Clamp(timeCoefficient, 0, 1);
        heroCoefficient *= Math.Clamp(timeCoefficient, 0, 1);

        var rand = Random.Shared.NextDouble();

        if (EngineerAgent.Distance > 0.5f)
            return false;

        if (rand < patrolCoefficient)
            state = Patrol;
        else if (rand < heroCoefficient)
            state = HeroTracker;
        else
            return false;
        return true;
    }
}