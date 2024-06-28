using System.Numerics;
using AllianceDM.PreInfo;

namespace AllianceDM.ALPlanner;

class Aggression(IStateMechine aggressionMechine, DecisionMakingInfo info) : IStateObject
{
    public bool FirePermit { get; private set; }
    public bool[] LockPermit { get; private set; }
    public Vector2 GimbalAngle { get; private set; }
    public Vector2 TargetPosition { get; private set; }

    IStateMechine aggressionMechine = aggressionMechine;
    DecisionMakingInfo info;

    public IStateObject Update(float timeCoefficient)
    {

    }
}