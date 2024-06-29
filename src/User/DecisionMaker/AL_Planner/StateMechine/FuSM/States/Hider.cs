using System.Numerics;

namespace AllianceDM.ALPlanner;
class Hider(float hideTime) : IStateObject
{
    public bool FirePermit => true;

    public bool[] LockPermit { get; private set; } = [true, true, true, true, true, true, true];

    public Vector2 GimbalAngle => new(0f, -0.1f);

    public Vector2 TargetPosition { get; private set; }


    public required HeroAgent HeroAgent { get; init; }
    public required UVAAgent UVAAgent { get; init; }
    public required JumperAgent JumperAgent { get; init; }
    public IStateObject GreatWallWatcher { get; set; }

    static readonly Vector2 hidePosition_ = new(-3.27f, 5.64f);
    static readonly Vector2 interceptionPosition_ = new(-2.55f, 6.69f);
    float current_ = DateTime.Now.Ticks;
    float hideTime_ = hideTime;

    public bool Update(ref IStateObject state, float timeCoefficient)
    {
        if (JumperAgent.Detected)
            TargetPosition = interceptionPosition_;
        else
            TargetPosition = hidePosition_;
        if (DateTime.Now.Ticks - current_ < hideTime_)
            return false;
        current_ = DateTime.Now.Ticks;
        if (UVAAgent.AirSupport)
            return false;

        if (HeroAgent.Position.X < 0)
            state = GreatWallWatcher;

        return true;
    }
}
