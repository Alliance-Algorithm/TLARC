using System.Numerics;

namespace AllianceDM.ALPlanner;

class HeroAgent : Component
{
    public Vector2 Position => targetPreDictor.Position;
    public bool Locked => targetPreDictor.Locked;
    public bool Found => targetPreDictor.Found;
    public float Angle => targetPreDictor.Angle;
    public float Value { get; private set; }

    public float Distance => targetPreDictor.Distance;
    EngineerTargetPreDictor targetPreDictor;

    public override void Start()
    {
        Value = Math.Clamp(targetPreDictor.Distance * (targetPreDictor.Found ? 10 : 1) + 100 * (Locked ? 1 : 0), 0, float.PositiveInfinity);

    }
}