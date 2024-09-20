
using DecisionMaker.Predictor;

namespace DecisionMaker.StateMachine.FuSM.Status;

class Portal : IStateObject
{
    public bool FirePermit => true;

    public bool[] LockPermit => [true, true, true, true, true, true, true];

    public Vector2d GimbalAngle => new(0, 0);

    public Vector2d TargetPosition { get; private set; }

    public required Transform Sentry { get; init; }

    static readonly Vector3d[] positions_ = [new(-9.24f, 2.05f, 0), new(-7.37f, 2.12f, 0), new(-9.55f, 0.02f, 0)];
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
        TargetPosition = new(positions_[iterator_].x, positions_[iterator_].x);
        if ((Sentry.Position - positions_[iterator_]).Length > 2)
            _timeTick = DateTime.Now.Ticks;
        if ((Sentry.Position - positions_[iterator_]).Length < 0.3f || (DateTime.Now.Ticks - _timeTick) / 1e7f > 2)
        {
            _timeTick = DateTime.Now.Ticks;
            iterator_ = (iterator_ + 1) % positions_.Length;
            TargetPosition = new(positions_[iterator_].x, positions_[iterator_].x);
        }

        if (JumperAgent.Detected || (HeroAgent.Position.x < 0 && HeroAgent.EquivalentHp < 1000))
            state = GreatWallWatcher;
        else if (UVAAgent.AirSupport)
            state = Hider;
        else
            return false;

        return false;

    }
}