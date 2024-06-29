using System.Numerics;

namespace AllianceDM.ALPlanner;

class EngineerInterceptionPoint : Component
{
    public Vector2 Position => targetPreDictor.Position;
    public bool Locked => targetPreDictor.Locked;
    public float Angle => targetPreDictor.Angle;
    public float Value { get; private set; }

    EngineerTargetPreDictor targetPreDictor;

    public override void Start()
    {
        Value = 10 * (targetPreDictor.Found ? 1 : 0) + 100 * (Locked ? 1 : 0);
    }
}