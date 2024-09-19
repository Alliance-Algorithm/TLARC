using Rosidl.Messages.Builtin;

namespace TrajectoryTracer;

interface ITrajectory
{
    public Vector3d Position { get; }
}