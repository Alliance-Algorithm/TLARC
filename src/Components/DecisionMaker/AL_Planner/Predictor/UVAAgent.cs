using Tlarc.PreInfo;

namespace Tlarc.ALPlanner;

class UVAAgent : Component
{
    EnemyUnitInfo unitInfo;
    public bool AirSupport => unitInfo.AirSupport;
}