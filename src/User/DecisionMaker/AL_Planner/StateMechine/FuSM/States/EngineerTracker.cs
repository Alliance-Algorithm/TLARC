using System.Numerics;
using AllianceDM.PreInfo;
using AllianceDM.StdComponent;

namespace AllianceDM.ALPlanner;

class EngineerTracker : IStateObject
{
    public bool FirePermit { get; private set; } = false;

    public bool[] LockPermit { get; private set; } = [true, true, false, false, false, false, false];

    public Vector2 GimbalAngle { get; private set; }

    public Vector2 TargetPosition => EngineerInterceptionPoint.Position;

    public required HeroInterceptionPoint HeroInterceptionPoint { get; init; }
    public required EngineerInterceptionPoint EngineerInterceptionPoint { get; init; }
    public required DecisionMakingInfo Info { get; init; }

    public IStateObject Patrol { get; set; }
    public IStateObject HeroTracker { get; set; }

    float OutpostHp = DecisionMakingInfo.OutpostHPLimit;

    public bool Update(ref IStateObject state, float timeCoefficient)
    {
        FirePermit = (EngineerInterceptionPoint.Distance < 6) && EngineerInterceptionPoint.Locked;
        float engineerCoefficient = EngineerInterceptionPoint.Value;
        float heroCoefficient = HeroInterceptionPoint.Value + (OutpostHp == Info.FriendOutPostHp ? 100 : 0);
        float patrolCoefficient = (HeroInterceptionPoint.Found ? 1 : 10) * (EngineerInterceptionPoint.Found ? 1 : 10)
        / (HeroInterceptionPoint.Value + EngineerInterceptionPoint.Value);

        float total = heroCoefficient + engineerCoefficient + patrolCoefficient;
        engineerCoefficient /= total;
        heroCoefficient = heroCoefficient / total + engineerCoefficient;

        var rand = Random.Shared.NextDouble();

        if (rand < engineerCoefficient)
            return false;
        else if (rand < heroCoefficient)
            state = HeroTracker;
        else
            state = Patrol;
        return true;
    }
}