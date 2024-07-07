using System.Numerics;
using AllianceDM.PreInfo;
using AllianceDM.StdComponent;
using Rosidl.Messages.Std;

namespace AllianceDM.ALPlanner;

class HeroTargetPreDictor : Component
{
    int VehicleCode { get; init; } = (int)RobotType.Engineer;

    public bool Found => unitInfo.Found[VehicleCode];
    public bool Locked { get; private set; }
    public Vector2 Position { get; private set; } = new Vector2(2.88f, -6.8f);
    public float Distance { get; private set; }
    public float Angle { get; private set; }

    Transform2D sentry;
    EnemyUnitInfo unitInfo;

    private int presetIndex_ = 0;
    private long timeTick_ = 0;

    Vector2[] positions_ = [new(7.68f, -1.8f), new(0.49f, 3.5f)];
    float[] angles_ = { -MathF.PI, 0 };

    public override void Update()
    {
        Locked = unitInfo.Locked == 1;
        if (Found)
        {
            Position = unitInfo.Position[VehicleCode];
            var trans = Position - sentry.position;
            Angle = MathF.Atan(trans.Y / trans.X);
            return;
        }

        Position = positions_[presetIndex_];
        if ((sentry.position - Position).Length() < 0.1f)
            if (Angle != angles_[presetIndex_])
            {
                Angle = angles_[presetIndex_];
                timeTick_ = DateTime.Now.Ticks;
                return;
            }
            else if ((DateTime.Now.Ticks - timeTick_) * 1e-7 > 1)
                presetIndex_ = (presetIndex_ + 1) % 2;

        Distance = (Position - sentry.position).Length();
    }
}