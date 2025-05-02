namespace RapidlyArmPlanner.ColliderDetector;

interface IColliderDetector
{
  public bool Detect(List<(Vector3d position, Quaterniond rotation)> poses);

  public void Update(
    List<(Vector3d Position, Quaterniond Rotation)> poleTransforms,
    (Vector3d Position, Quaterniond Rotation) redemptionTransform
  );
}
