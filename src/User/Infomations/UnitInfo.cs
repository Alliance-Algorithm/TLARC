using System.Numerics;

namespace AllianceDM.ALPlanner;

class UnitInfo : Component
{
    public bool[] Found { get; private set; }
    public Vector2[] Position { get; private set; }
    public int Locked { get; private set; }
}