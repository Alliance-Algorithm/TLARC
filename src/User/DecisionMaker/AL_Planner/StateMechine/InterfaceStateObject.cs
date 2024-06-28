using System.Numerics;
namespace AllianceDM.ALPlanner;

interface IStateObject
{
    IStateObject Update(float timeCoefficient);
    public bool FirePermit { get; }
    public bool[] LockPermit { get; }
    public Vector2 GimbalAngle { get; }
    public Vector2 TargetPosition { get; }
}