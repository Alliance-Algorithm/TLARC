using AllianceDM.PreInfo;

namespace AllianceDM.ALPlanner;

class UVAAgent : Component
{
    EnemyUnitInfo unitInfo;
    public bool AirSupport => unitInfo.AirSupport;
}