using DecisionMaker.Information;

namespace DecisionMaker.Predictor;

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

  private bool CheckPosition(Vector2d pos)
  {
    return pos.x < 2.5f && pos.x > 1 && pos.y > 6.5;
  }
}
