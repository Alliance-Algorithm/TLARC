using System.Numerics;

namespace AllianceDM.ALPlanner;

interface IStateMechine
{
    public bool FirePermit { get; set; }
    public bool[] LockPermit { get; set; }
    public Vector2 GimbalAngle { get; set; }
    public Vector2 TargetPosition { get; set; }
    IStateObject[] AllState { get; set; }

    IStateObject AnyState();
}