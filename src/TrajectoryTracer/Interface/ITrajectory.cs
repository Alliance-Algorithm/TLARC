using Rosidl.Messages.Builtin;

namespace TrajectoryTracer;

interface ITrajectory<T>
{
    public T Position { get; }
}