using System.Numerics;
using DecisionMaker.Information;
using DecisionMaker.Predictor;

namespace DecisionMaker.StateMachine.FuSM.Status;

class GreatWallWatcher : IStateObject
{
    public bool FirePermit => true;

    public bool[] LockPermit { get; private set; } = [true, false, true, true, true, true, true, true];

    public Vector2d GimbalAngle { get; private set; }

    public Vector2d TargetPosition { get; private set; }

    public required HeroAgent HeroAgent { get; init; }
    public required UVAAgent UVAAgent { get; init; }
    public required JumperAgent JumperAgent { get; init; }

    static readonly Vector2d hidePosition_ = new(-3.27f, 5.64f);
    static readonly Vector2d hightWallPosition_ = new(-3.6f, 2.79f);
    required public DecisionMakingInfo Info { get; init; }
    public IStateObject Portal { get; set; }
    public IStateObject Hider { get; set; }
    public bool Update(ref IStateObject state, float timeCoefficient)
    {

        if (Info.BaseArmorOpeningCountdown <= 15)
        {
            state = Portal;
            return true;
        }
        if (UVAAgent.AirSupport)
        {
            state = Hider;
            return true;
        }

        if (timeCoefficient < 1f)
            return false;

        if (HeroAgent.Position.x < 0)
        {
            TargetPosition = hightWallPosition_;
            GimbalAngle = new(-MathF.PI * 3 / 4 - 1f, -MathF.PI * 3 / 4 + 1f);
            for (int i = 0; i < 8; i++)
            {
                LockPermit[i] = false;
            }
            LockPermit[0] = true;
        }
        else if (JumperAgent.Detected)
        {
            TargetPosition = hidePosition_;
            GimbalAngle = new(-2.3f, -2.1f);
            for (int i = 0; i < 8; i++)
            {
                LockPermit[i] = true;
            }
            LockPermit[1] = false;
        }
        if (!JumperAgent.Detected && (!(HeroAgent.Position.x < 0) || HeroAgent.EquivalentHp > 1000))
            state = Portal;
        else
            return false;

        return true;

    }
}