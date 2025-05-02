namespace AutoExchange.RedemptionDetector;

interface IRedemptionDetector
{
  public (Vector3d position, Quaterniond rotation) GetRedemptionInCamera();
}
