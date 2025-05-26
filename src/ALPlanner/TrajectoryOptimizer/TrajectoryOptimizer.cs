namespace ALPlanner.TrajectoryOptimizer;

class TrajectoryOptimizer : Component
{
  [ComponentReferenceFiled]
  IKOrderCurve kOrderCurve;

  public DateTime constructTime { get; private set; }
  public double constructTimeToNowInSeconds =>
    (DateTime.Now - constructTime).Duration().TotalSeconds;

  public double MaxTime => kOrderCurve.MaxTime;

  public void CalcTrajectory(ConstraintCollection constraints)
  {
    constructTime = DateTime.Now;
    kOrderCurve.Construction(constraints);
  }
  public void CalcTrajectory(Vector3d point)
  {
    constructTime = DateTime.Now;
    kOrderCurve.Construction(point);
  }

  public void OptimizeTrajectory()
  {
    kOrderCurve.OptimizeTrajectory();
  }

  public void Construction(
    Vector3d[] positionList,
    Vector3dTuple2 HeadTailVelocity,
    Vector3dTuple2 HeadTailAcceleration
  )
  {
    throw new NotImplementedException();
  }

  public IEnumerable<Vector3d> TrajectoryPoints(double fromWhen, double toWhen, double step) =>
    kOrderCurve.TrajectoryPoints(fromWhen, toWhen, step);

  public override void Update() { }

  public bool Check() => kOrderCurve.Check();
}
