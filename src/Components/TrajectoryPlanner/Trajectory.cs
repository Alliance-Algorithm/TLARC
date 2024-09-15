
using System.Numerics;

namespace Tlarc.TrajectoryPlanner;

abstract class Trajectory : Component
{
    public abstract Vector2 TargetVelocity { get; }
    public abstract Vector2 TargetAccelerate { get; }
    public abstract Vector2 TargetPosition { get; }
}