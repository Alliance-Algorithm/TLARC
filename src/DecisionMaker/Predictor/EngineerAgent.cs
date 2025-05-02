using DecisionMaker.Information;

namespace DecisionMaker.Predictor;

class EngineerAgent : Component
{
  public Vector2d Position => targetPreDictor.Position;
  public bool Locked => targetPreDictor.Locked;
  public bool Found => targetPreDictor.Found;
  public double Angle => targetPreDictor.Angle;
  public double Value { get; private set; }

  public double Distance => targetPreDictor.Distance;
  EngineerTargetPreDictor targetPreDictor;
  EnemyUnitInfo unitInfo;

  public override void Update()
  {
    Value =
      Math.Clamp(
        (targetPreDictor.Found ? 200 : 1)
          + 100 * (Locked ? 1 : 0)
          - unitInfo.EquivalentHp[(int)RobotType.Engineer] / 100f,
        0,
        float.PositiveInfinity
      ) + 1;
  }
}
