

namespace ALPlanner.TrajectoryOptimizer;

interface IKOrderCurve
{
  public double MaxTime { get; }
  public DateTime ConstructTime { get; }
  public IEnumerable<Vector3d> TrajectoryPoints(double fromWhen, double toWhen, double step);
  public IEnumerable<Vector3d> VelocitiesPoints(double fromWhen, double toWhen, double step);

  public void Construction(ConstraintCollection positionList);
  public void Construction(Vector3d point);
  public void OptimizeTrajectory();
  public bool Check();
}
