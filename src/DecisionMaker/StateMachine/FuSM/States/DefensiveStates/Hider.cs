using System.Numerics;
using DecisionMaker.Predictor;

namespace DecisionMaker.StateMachine.FuSM.Status;
class Hider(float hideTime) : IStateObject
{
    public bool FirePermit => true;

    public bool[] LockPermit { get; private set; } = [true, true, true, true, true, true, true];

    public Vector2d GimbalAngle { get; private set; }

    public Vector2d TargetPosition { get; private set; }


    public required HeroAgent HeroAgent { get; init; }
    public required UVAAgent UVAAgent { get; init; }
    public required JumperAgent JumperAgent { get; init; }
    public IStateObject GreatWallWatcher { get; set; }
    public IStateObject Portal { get; set; }

    static readonly Vector2d hidePosition_ = new(-3.27f, 5.64f);
    static readonly Vector2d interceptionPosition_ = new(-2.55f, 6.69f);
    float current_ = DateTime.Now.Ticks;
    float hideTime_ = hideTime;

    public bool Update(ref IStateObject state, float timeCoefficient)
    {
        if (JumperAgent.Detected)
        {
            TargetPosition = interceptionPosition_;
            GimbalAngle = new(0, 0.1f);
        }
        else
        {
            TargetPosition = hidePosition_;
            GimbalAngle = new(0, 0);
        }
        if (DateTime.Now.Ticks - current_ < hideTime_)
            return false;
        current_ = DateTime.Now.Ticks;
        if (UVAAgent.AirSupport)
            return false;

        if (JumperAgent.Detected || HeroAgent.Position.x < 0)
            state = GreatWallWatcher;
        else
            state = Portal;

        return true;
    }
}
