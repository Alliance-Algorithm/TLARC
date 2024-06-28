using System.Numerics;

namespace AllianceDM.ALPlanner;

class UnitInterceptionPoint : Component
{
    public Vector2 Position { get; private set; }
    public float Value { get; private set; }
}