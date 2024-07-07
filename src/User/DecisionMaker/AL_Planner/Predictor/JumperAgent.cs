using AllianceDM.PreInfo;

namespace AllianceDM.ALPlanner;

class JumperAgent : Component
{
    EnemyUnitInfo unitInfo;
    public bool Detected { get; private set; }
}