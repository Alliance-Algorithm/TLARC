using System.Numerics;
namespace DecisionMaker.StateMachine;

interface IStateObject
{
    bool Update(ref IStateObject state, float timeCoefficient);

    public bool FirePermit { get; }
    public bool[] LockPermit { get; }
    public Vector2d GimbalAngle { get; }
    public Vector2d TargetPosition { get; }
}