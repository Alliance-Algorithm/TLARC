using Rosidl.Messages.Builtin;

namespace TrajectoryTracer.Trajectory;

interface ITrajectory<T>
{
    public T[] Trajectory(double howLong, int count);
}