namespace AllianceDM.ALPlanner;

interface IStateMechine
{
    public bool FirePermit { get; private set}
    public bool[] LockPermit { get; private set}
    public Vector2 GimbalAngle { get; private set; }
    public Vector2 TargetPosition { get; private set; };
    IStateObject[] AllState;

    IStateObject AnyState()
    {

    }
}