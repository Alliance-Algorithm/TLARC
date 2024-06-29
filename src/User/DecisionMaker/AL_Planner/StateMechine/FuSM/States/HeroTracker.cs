using System.Numerics;

namespace AllianceDM.ALPlanner;

class HeroTracker : IStateObject
{
    public bool FirePermit => throw new NotImplementedException();

    public bool[] LockPermit => throw new NotImplementedException();

    public Vector2 GimbalAngle => throw new NotImplementedException();

    public Vector2 TargetPosition => throw new NotImplementedException();

    public required EngineerInterceptionPoint heroPoint;

    public bool Update(ref IStateObject state, float timeCoefficient)
    {

        throw new NotImplementedException();
    }
}