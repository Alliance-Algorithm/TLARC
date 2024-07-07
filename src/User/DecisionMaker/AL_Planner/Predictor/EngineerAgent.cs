using System.Numerics;
using AllianceDM.PreInfo;

namespace AllianceDM.ALPlanner;

class EngineerAgent : Component
{
    public Vector2 Position => targetPreDictor.Position;
    public bool Locked => targetPreDictor.Locked;
    public bool Found => targetPreDictor.Found;
    public float Angle => targetPreDictor.Angle;
    public float Value { get; private set; }

    public float Distance => targetPreDictor.Distance;
    EngineerTargetPreDictor targetPreDictor;
    EnemyUnitInfo unitInfo;
    public override void Update()
    {
        Value = Math.Clamp(targetPreDictor.Distance * (targetPreDictor.Found ? 10 : 1) + 100 * (Locked ? 1 : 0) - unitInfo.EquivalentHp[(int)RobotType.Engineer] / 100f, 0, float.PositiveInfinity) + 1;
    }
}