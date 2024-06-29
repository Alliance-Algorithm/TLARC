using System.Numerics;

namespace AllianceDM.ALPlanner;

class Defensive_FuSM : Component, IStateMachine
{
    public bool FirePermit => throw new NotImplementedException();

    public bool[] LockPermit => throw new NotImplementedException();

    public Vector2 GimbalAngle => throw new NotImplementedException();

    public Vector2 TargetPosition => throw new NotImplementedException();

    public IStateObject[] AllState => throw new NotImplementedException();

    public IStateObject AnyState()
    {
        throw new NotImplementedException();
    }
}