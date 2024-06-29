using System.Numerics;
using AllianceDM.PreInfo;

namespace AllianceDM.ALPlanner;
class PatrolState : IStateObject
{
    public bool FirePermit { get; private set; }

    public bool[] LockPermit => [true, true, false, false, false, false, false];

    public Vector2 GimbalAngle { get; private set; }

    public Vector2 TargetPosition => throw new NotImplementedException();


    public required HeroAgent HeroAgent { get; init; }
    public required EngineerAgent EngineerAgent { get; init; }
    public required DecisionMakingInfo DecisionMakingInfo { get; init; }
    public required UnitInfo UnitInfo { get; init; }
    float OutpostHp = DecisionMakingInfo.OutpostHPLimit;
    public IStateObject EngineerTracker { get; set; }
    public IStateObject HeroTracker { get; set; }

    public bool Update(ref IStateObject state, float timeCoefficient)
    {
        LockPermit[(int)RobotType.Hero] = UnitInfo.EquivalentHp[(int)RobotType.Hero] < 1000;
        LockPermit[(int)RobotType.Engineer] = UnitInfo.EquivalentHp[(int)RobotType.Engineer] < 1000;
        GimbalAngle = new(EngineerAgent.Angle - MathF.PI, EngineerAgent.Angle + MathF.PI);
        FirePermit = (EngineerAgent.Distance < 6) && EngineerAgent.Locked;
        float engineerCoefficient = EngineerAgent.Value;
        float heroCoefficient = HeroAgent.Value + (OutpostHp == DecisionMakingInfo.FriendOutPostHp ? 100 : 0);
        float patrolCoefficient = (HeroAgent.Found ? 1 : 10) * (EngineerAgent.Found ? 1 : 10)
        / (HeroAgent.Value + EngineerAgent.Value);


        float total = heroCoefficient + engineerCoefficient + patrolCoefficient;
        engineerCoefficient /= total;
        heroCoefficient = heroCoefficient / total + engineerCoefficient;


        engineerCoefficient *= Math.Clamp(timeCoefficient, 0, 1);
        heroCoefficient *= Math.Clamp(timeCoefficient, 0, 1);

        var rand = Random.Shared.NextDouble();

        if (rand < engineerCoefficient)
            state = EngineerTracker;
        else if (rand < heroCoefficient)
            state = HeroTracker;
        else
            return false;
        return true;
    }
}