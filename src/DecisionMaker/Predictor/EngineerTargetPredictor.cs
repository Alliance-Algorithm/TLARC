using DecisionMaker.Information;

namespace DecisionMaker.Predictor;

class EngineerTargetPreDictor : Component
{
  int VehicleCode { get; init; } = (int)RobotType.Engineer;

  public bool Found => unitInfo.Found[VehicleCode];
  public bool Locked { get; private set; }
  public Vector2d Position { get; private set; } = new(5.98f, -2.8f);
  public double Distance { get; private set; }
  public double Angle { get; private set; }

  Transform sentry;
  EnemyUnitInfo unitInfo;

  private int presetIndex_ = 0;
  private long timeTick_ = 0;
  private long timeTick2_ = DateTime.Now.Ticks;

  Vector3d[] Positions_ = [new(5.98f, -2.8f, 0), new(0.49f, 3.5f, 0)];
  float[] angles_ = { -MathF.PI, 0 };

  public override void Update()
  {
    Locked = unitInfo.Locked == 2;
    if (!Found)
      Position = Positions_[presetIndex_].xy;
    if (Found && DateTime.Now.Ticks - timeTick2_ > 2e7f)
    {
      Position = unitInfo.Position[VehicleCode];
      timeTick2_ = DateTime.Now.Ticks;
      return;
    }
    var trans = Position - sentry.Position.xy;
    Angle = Math.Atan2(trans.y, trans.x);

    if ((sentry.Position.xy - Position).Length < 0.1f)
      if (Angle != angles_[presetIndex_])
      {
        Angle = angles_[presetIndex_];
        timeTick_ = DateTime.Now.Ticks;
        return;
      }
      else if ((DateTime.Now.Ticks - timeTick_) * 1e-7 > 1)
        presetIndex_ = (presetIndex_ + 1) % 2;
    if (Found)
      Distance = (sentry.Position.xy - unitInfo.Position[VehicleCode]).Length;
    else
      Distance = (sentry.Position.xy - Position).Length;
    if (Distance < 1.5f)
      Position = sentry.Position.xy;
    Distance = (sentry.Position.xy - Position).Length;
  }
}
