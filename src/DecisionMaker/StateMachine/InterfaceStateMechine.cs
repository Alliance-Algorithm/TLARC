

namespace DecisionMaker.StateMachine;

interface IStateMachine
{
    public bool FirePermit { get; }
    public bool[] LockPermit { get; }
    public Vector2d GimbalAngle { get; }
    public Vector2d TargetPosition { get; }

    IStateObject AnyState();
}