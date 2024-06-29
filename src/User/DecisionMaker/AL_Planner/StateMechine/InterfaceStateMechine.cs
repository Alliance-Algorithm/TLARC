using System.Numerics;

namespace AllianceDM.ALPlanner;

interface IStateMachine
{
    public bool FirePermit { get; }
    public bool[] LockPermit { get; }
    public Vector2 GimbalAngle { get; }
    public Vector2 TargetPosition { get; }

    IStateObject AnyState();
}