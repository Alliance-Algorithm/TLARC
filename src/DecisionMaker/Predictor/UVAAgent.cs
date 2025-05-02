using DecisionMaker.Information;

namespace DecisionMaker.Predictor;

class UVAAgent : Component
{
  EnemyUnitInfo unitInfo;
  public bool AirSupport => unitInfo.AirSupport;
}
