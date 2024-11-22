namespace RapidlyArmPlanner.ColliderDetector;
interface IColliderDetector
{
    public bool Detect(List<(Vector3d position, Quaterniond rotation)> poses);

}