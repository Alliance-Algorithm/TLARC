using System.Numerics;

namespace AllianceDM.PreInfo;

class EnemyUnitInfo : Component
{
    public bool[] Found { get; private set; } = new bool[7];
    public Vector2[] Position { get; private set; } = new Vector2[7];
    public int Locked { get; private set; } = -1;
    public float[] Hp { get; private set; } = new float[7];
    public float[] EquivalentHp { get; private set; } = new float[7];
    public bool AirSupport { get; private set; } = false;
    public bool JumpPointDetect { get; private set; } = false;
}