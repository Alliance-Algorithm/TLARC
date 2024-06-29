using System.Numerics;

namespace AllianceDM.ALPlanner;

class GreatWallWatcher : IStateObject
{
    public bool FirePermit => true;

    public bool[] LockPermit { get; private set; } = [true, false, true, true, true, true, true, true];

    public Vector2 GimbalAngle { get; private set; }

    public Vector2 TargetPosition { get; private set; }

    public required HeroAgent HeroAgent { get; init; }
    public required UVAAgent UVAAgent { get; init; }
    public required JumperAgent JumperAgent { get; init; }

    static readonly Vector2 hidePosition_ = new(-3.27f, 5.64f);
    static readonly Vector2 hightWallPosition_ = new(-2.08f, 7.13f);

    public IStateObject Portal { get; set; }
    public IStateObject Hider { get; set; }
    public bool Update(ref IStateObject state, float timeCoefficient)
    {
        if (UVAAgent.AirSupport)
        {
            state = Hider;
            return true;
        }

        if (timeCoefficient < 1f)
            return false;

        if (HeroAgent.Position.X < 0)
        {
            TargetPosition = hightWallPosition_;
            GimbalAngle = new(-0.1f, 0.1f);
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
        if (!JumperAgent.Detected && !(HeroAgent.Position.X < 0))
            state = Portal;
        else
            return false;

        return true;

    }
}