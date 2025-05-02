namespace RapidlyArmPlanner.ArmSolver.InverseDynamic;

interface IInverseDynamicSolver
{
  bool Solve((Vector3d position, Quaterniond forward) target, out List<double[]> thetas);
}
