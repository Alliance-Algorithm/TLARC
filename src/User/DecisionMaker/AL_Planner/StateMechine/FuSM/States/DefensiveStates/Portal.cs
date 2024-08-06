using System.Numerics;
using AllianceDM.StdComponent;

namespace AllianceDM.ALPlanner;

class Portal : IStateObject
{
    public bool FirePermit => true;

    public bool[] LockPermit => [true, true, true, true, true, true, true];

    public Vector2 GimbalAngle => new(0, 0);

    public Vector2 TargetPosition { get; private set; }

    public required Transform2D Sentry { get; init; }

    static readonly Vector2[] positions_ = [new(-9.24f, 1.05f), new(-7.37f, 1.12f), new(-9.55f, -1.02f)];
    int iterator_ = 0;
    long timer_ = DateTime.Now.Ticks;
    public required HeroAgent HeroAgent { get; init; }
    public required UVAAgent UVAAgent { get; init; }
    public required JumperAgent JumperAgent { get; init; }
    public IStateObject GreatWallWatcher { get; set; }
    public IStateObject Hider { get; set; }

    private long _timeTick = DateTime.Now.Ticks;

    public bool Update(ref IStateObject state, float timeCoefficient)
    {
        TargetPosition = positions_[iterator_];
        if ((Sentry.position - positions_[iterator_]).Length() > 2)
            _timeTick = DateTime.Now.Ticks;
        if ((Sentry.position - positions_[iterator_]).Length() < 0.3f || (DateTime.Now.Ticks - _timeTick) / 1e7f > 2)
        {
            _timeTick = DateTime.Now.Ticks;
            iterator_ = (iterator_ + 1) % positions_.Length;
            TargetPosition = positions_[iterator_];
        }

        if (JumperAgent.Detected || (HeroAgent.Position.X < 0 && HeroAgent.EquivalentHp < 1000))
            state = GreatWallWatcher;
        else if (UVAAgent.AirSupport)
            state = Hider;
        else
            return false;

        return false;

    }
}