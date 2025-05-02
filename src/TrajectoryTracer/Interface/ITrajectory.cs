using Rosidl.Messages.Builtin;

namespace TrajectoryTracer.Trajectory;

interface ITrajectory<T>
{
  public T[] Trajectory(double howLong, int count);
  public T[] Velocities(double howLong, int count);
}
