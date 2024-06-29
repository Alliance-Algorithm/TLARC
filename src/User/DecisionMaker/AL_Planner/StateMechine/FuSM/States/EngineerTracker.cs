using System.Numerics;
using AllianceDM.StdComponent;

namespace AllianceDM.ALPlanner;

class EngineerTracker : IStateObject
{
    public bool FirePermit { get; private set; } = false;

    public bool[] LockPermit => [true, true, false, false, false, false, false];

    public Vector2 GimbalAngle { get; private set; }

    public Vector2 TargetPosition => EngineerInterceptionPoint.Position;

    public required EngineerInterceptionPoint HeroInterceptionPoint { get; init; }
    public required EngineerInterceptionPoint EngineerInterceptionPoint { get; init; }
    public required Transform2D Sentry { get; init; }

    public bool Update(ref IStateObject state, float timeCoefficient)
    {
        throw new NotImplementedException();
    }
}