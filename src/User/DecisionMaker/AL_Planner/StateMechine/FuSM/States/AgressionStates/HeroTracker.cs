using System.Numerics;
using AllianceDM.PreInfo;

namespace AllianceDM.ALPlanner;

class HeroTracker : IStateObject
{
    public bool FirePermit { get; private set; }

    public bool[] LockPermit { get; private set; } = [true, true, false, false, false, false, false];
    public Vector2 GimbalAngle { get; private set; }

    public Vector2 TargetPosition => HeroAgent.Position;
    public required HeroAgent HeroAgent { get; init; }
    public required EngineerAgent EngineerAgent { get; init; }
    public required DecisionMakingInfo DecisionMakingInfo { get; init; }
    public required EnemyUnitInfo UnitInfo { get; init; }


    public IStateObject Patrol { get; set; }
    public IStateObject EngineerTracker { get; set; }
    float OutpostHp = DecisionMakingInfo.OutpostHPLimit;
    public bool Update(ref IStateObject state, float timeCoefficient)
    {
        LockPermit[(int)RobotType.Hero] = UnitInfo.EquivalentHp[(int)RobotType.Hero] < 1000;
        LockPermit[(int)RobotType.Engineer] = UnitInfo.EquivalentHp[(int)RobotType.Engineer] < 100;
        GimbalAngle = new(HeroAgent.Angle - MathF.PI / 4, HeroAgent.Angle + MathF.PI / 4);
        FirePermit = (HeroAgent.Distance < 6) && HeroAgent.Locked;
        float engineerCoefficient = EngineerAgent.Value;
        float heroCoefficient = HeroAgent.Value + (OutpostHp != DecisionMakingInfo.FriendOutPostHp ? 100 : 0);
        float patrolCoefficient = (HeroAgent.Found ? 1 : 10) * (EngineerAgent.Found ? 1 : 10);


        float total = heroCoefficient + engineerCoefficient + patrolCoefficient;
        engineerCoefficient /= total;
        patrolCoefficient = patrolCoefficient / total + engineerCoefficient;


        engineerCoefficient *= Math.Clamp(timeCoefficient / 10, 0, 1);
        patrolCoefficient *= Math.Clamp(timeCoefficient / 10, 0, 1);

        if (HeroAgent.Distance > 1.5f)
            return false;
        var rand = Random.Shared.NextDouble();

        if (rand < engineerCoefficient)
            state = EngineerTracker;
        else if (rand < patrolCoefficient)
            state = Patrol;
        else
            return false;
        return true;
    }
}