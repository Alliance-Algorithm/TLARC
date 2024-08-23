using System.Numerics;
namespace Tlarc.ALPlanner;

interface IStateObject
{
    bool Update(ref IStateObject state, float timeCoefficient);

    public bool FirePermit { get; }
    public bool[] LockPermit { get; }
    public Vector2 GimbalAngle { get; }
    public Vector2 TargetPosition { get; }
}