using System.Numerics;

namespace AllianceDM.PreInfo;

class UnitInfo : Component
{
    public bool[] Found { get; private set; }
    public Vector2[] Position { get; private set; }
    public int Locked { get; private set; }
    public float[] Hp { get; set; }
    public float[] EquivalentHp { get; set; }
}