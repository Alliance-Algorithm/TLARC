using System.Numerics;
namespace AllianceDM.ALPlanner;

interface IStateObject
{
    IStateObject Update();
    public bool FirePermit { get; private set}
    public bool[] LockPermit { get; private set}
    public Vector2 GimbalAngle { get; private set; }
    public Vector2 TargetPosition { get; private set; };
}