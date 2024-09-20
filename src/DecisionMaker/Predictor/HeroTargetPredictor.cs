using DecisionMaker.Information;
namespace DecisionMaker.Predictor;

class HeroTargetPreDictor : Component
{
    int VehicleCode { get; init; } = (int)RobotType.Hero;

    public bool Found => unitInfo.Found[VehicleCode];
    public bool Locked { get; private set; }
    public Vector2d Position { get; private set; } = new Vector2d(2.88, -6.8);
    public double Distance { get; private set; }
    public double Angle { get; private set; }

    Transform sentry;
    EnemyUnitInfo unitInfo;

    private int presetIndex_ = 0;
    private long timeTick_ = DateTime.Now.Ticks;

    Vector2d[] positions_ = [new(1.3f, -7.03f)];
    float[] angles_ = { -MathF.PI, 0 };

    public override void Update()
    {
        Locked = unitInfo.Locked == 1;
        if (!Found)
            Position = positions_[presetIndex_];
        if (Found && DateTime.Now.Ticks - timeTick_ > 2e7f)
        {
            Position = unitInfo.Position[VehicleCode];
            timeTick_ = DateTime.Now.Ticks;
            return;
        }
        var trans = new Vector3d(Position.x, Position.y, 0) - sentry.Position;
        Angle = Math.Atan2(trans.y, trans.x);
        if ((sentry.Position.xy - Position).Length < 0.1f)
            if (Angle != angles_[presetIndex_])
            {
                Angle = angles_[presetIndex_];
                timeTick_ = DateTime.Now.Ticks;
                return;
            }
        if (Found)
            Distance = (sentry.Position.xy - unitInfo.Position[VehicleCode]).Length;
        else
            Distance = (sentry.Position.xy - Position).Length;
        if (Distance < 1.5f)
            Position = sentry.Position.xy;
        Distance = (Position - sentry.Position.xy).Length;

    }
}