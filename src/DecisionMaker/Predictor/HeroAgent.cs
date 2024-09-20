using DecisionMaker.Information;

namespace DecisionMaker.Predictor;

class HeroAgent : Component
{
    public Vector2d Position => targetPreDictor.Position;
    public bool Locked => targetPreDictor.Locked;
    public bool Found => targetPreDictor.Found;
    public double Angle => targetPreDictor.Angle;
    public double Value { get; private set; }
    public double EquivalentHp => unitInfo.EquivalentHp[0];

    public double Distance => targetPreDictor.Distance;
    HeroTargetPreDictor targetPreDictor;
    EnemyUnitInfo unitInfo;
    public override void Update()
    {
        Value = Math.Clamp((targetPreDictor.Found ? 50 : 1) + 100 * (Locked ? 1 : 0) - unitInfo.EquivalentHp[(int)RobotType.Engineer] / 100f, 0, float.PositiveInfinity);

    }
}