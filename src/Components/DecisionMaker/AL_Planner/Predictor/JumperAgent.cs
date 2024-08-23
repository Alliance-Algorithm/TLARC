using System.Numerics;
using Tlarc.PreInfo;

namespace Tlarc.ALPlanner;

class JumperAgent : Component
{
    EnemyUnitInfo unitInfo;
    public bool Detected { get; private set; }

    public override void Update()
    {
        int i = 0;
        for (i = 0; i < 7; i++)
        {
            if (!unitInfo.Found[i])
                continue;
            if (!CheckPosition(unitInfo.Position[i]))
                continue;
            Detected = true;
            return;
        }
        Detected = false;
    }

    private bool CheckPosition(Vector2 pos)
    {
        return pos.X < 2.5f && pos.X > 1 && pos.Y > 6.5;
    }
}