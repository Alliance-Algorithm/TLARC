using System.Numerics;
using Tlarc.PreInfo;
using Tlarc.StdComponent;
using Rosidl.Messages.Std;

namespace Tlarc.ALPlanner;

class HeroTargetPreDictor : Component
{
    int VehicleCode { get; init; } = (int)RobotType.Hero;

    public bool Found => unitInfo.Found[VehicleCode];
    public bool Locked { get; private set; }
    public Vector2 Position { get; private set; } = new Vector2(2.88f, -6.8f);
    public float Distance { get; private set; }
    public float Angle { get; private set; }

    Transform2D sentry;
    EnemyUnitInfo unitInfo;

    private int presetIndex_ = 0;
    private long timeTick_ = DateTime.Now.Ticks;

    Vector2[] positions_ = [new(1.3f, -7.03f)];
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
        var trans = Position - sentry.Position;
        Angle = MathF.Atan2(trans.Y, trans.X);
        if ((sentry.Position - Position).Length() < 0.1f)
            if (Angle != angles_[presetIndex_])
            {
                Angle = angles_[presetIndex_];
                timeTick_ = DateTime.Now.Ticks;
                return;
            }
        if (Found)
            Distance = (sentry.Position - unitInfo.Position[VehicleCode]).Length();
        else
            Distance = (sentry.Position - Position).Length();
        if (Distance < 1.5f)
            Position = sentry.Position;
        Distance = (Position - sentry.Position).Length();
    }
}