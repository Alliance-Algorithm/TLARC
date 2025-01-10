using System.Collections.Generic;
using g4;
using RapidlyArmPlanner.ArmSolver.ForwardDynamic;
namespace RapidlyArmPlanner.ArmSolver.ForwardDynamic;

class SixAxis2025ForwardDynamic : IForwardDynamic
{
    double[] _pole = new double[] { 0.05, 0.3, 0.05, 0.3, 0.05 };
    public SixAxis2025ForwardDynamic(double[]? pole = null) { _pole = pole ?? _pole; }
    public List<(Vector3d pos, Quaterniond rotation)> GetPose(double[] angles)
    {
        List<(Vector3d pos, Quaterniond rotation)> ans = new()
        {
            (new(0, 0, _pole[0]/2), Quaterniond.AxisAngleR(Vector3d.AxisZ, angles[0]))
        };
        Vector3d jointPos = new(0, 0, _pole[0]);
        Quaterniond jointRot = ans[^1].rotation * Quaterniond.AxisAngleR(Vector3d.AxisY, angles[1]);
        ans.Add((jointPos + jointRot * Vector3d.AxisZ * _pole[1] / 2, jointRot));
        jointPos = jointPos + jointRot * Vector3d.AxisZ * _pole[1];
        jointRot = ans[^1].rotation * Quaterniond.AxisAngleR(Vector3d.AxisY, angles[2]);
        ans.Add((jointPos + jointRot * Vector3d.AxisX * _pole[2] / 2, jointRot));
        jointPos = jointPos + jointRot * Vector3d.AxisX * _pole[2];
        jointRot = ans[^1].rotation * Quaterniond.AxisAngleR(Vector3d.AxisZ, angles[3]);
        ans.Add((jointPos + jointRot * Vector3d.AxisZ * _pole[3] / 2, jointRot));
        jointPos = jointPos + jointRot * Vector3d.AxisZ * _pole[3];
        jointRot = ans[^1].rotation * Quaterniond.AxisAngleR(Vector3d.AxisY, angles[4]);
        ans.Add((jointPos + jointRot * Vector3d.AxisZ * _pole[4] / 2, jointRot));
        jointPos = jointPos + jointRot * Vector3d.AxisZ * _pole[4];
        jointRot = ans[^1].rotation * Quaterniond.AxisAngleR(Vector3d.AxisZ, angles[5]);
        ans.Add((jointPos + jointRot * Vector3d.AxisZ * 0.1, jointRot));
        return ans;
    }
}