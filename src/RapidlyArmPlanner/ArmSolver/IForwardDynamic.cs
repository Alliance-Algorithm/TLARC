namespace RapidlyArmPlanner.ArmSolver.ForwardDynamic;

interface IForwardDynamic
{
  List<(Vector3d pos, Quaterniond rotation)> GetPose(double[] angles);
}
