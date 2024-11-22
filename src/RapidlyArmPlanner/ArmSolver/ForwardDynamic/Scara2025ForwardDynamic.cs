namespace RapidlyArmPlanner.ArmSolver.ForwardDynamic;
class Scara2025ForwardDynamic : IForwardDynamic
{
    public Scara2025ForwardDynamic() { }
    public List<(Vector3d pos, Quaterniond rotation)> GetPose(double[] angles)
    {
        List<(Vector3d pos, Quaterniond rotation)> ans =
        [
            (new(0, 0, 0.5f), Quaterniond.AxisAngleR(Vector3d.AxisZ, angles[0]))
        ];
        ans.Add((new Vector3d(0, 0, (float)angles[1]) + ans[0].rotation * Vector3d.AxisX * 0.15, ans[0].rotation));
        var rotation = Quaterniond.AxisAngleR(Vector3d.AxisZ, angles[0] + angles[2]);
        ans.Add((new Vector3d(0, 0, (float)angles[1]) + ans[0].rotation * Vector3d.AxisX * 0.3 + rotation * Vector3d.AxisX * 0.15, rotation));
        rotation *= Quaterniond.AxisAngleR(Vector3d.AxisX, angles[3]);
        rotation *= Quaterniond.AxisAngleR(Vector3d.AxisZ, angles[4]);
        ans.Add((new Vector3d(0, 0, (float)angles[1]) + ans[0].rotation * Vector3d.AxisX * 0.3 + ans[2].rotation * Vector3d.AxisX * 0.3 + rotation * Vector3d.AxisX * 0.025, rotation));
        rotation *= Quaterniond.AxisAngleR(Vector3d.AxisX, angles[5]);
        ans.Add((new Vector3d(0, 0, (float)angles[1]) + ans[0].rotation * Vector3d.AxisX * 0.3 + ans[2].rotation * Vector3d.AxisX * 0.3 + ans[3].rotation * Vector3d.AxisX * 0.05 + rotation * Vector3d.AxisX * 0.11, rotation));

        return ans;
    }
}