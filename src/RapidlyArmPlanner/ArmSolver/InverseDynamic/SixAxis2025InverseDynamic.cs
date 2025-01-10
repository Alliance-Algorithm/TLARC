
using RapidlyArmPlanner.ArmSolver.InverseDynamic;
using g4;

namespace RapidlyArmPlanner.ArmSolver.ForwardDynamic;

class SixAxis2025InverseDynamic : IInverseDynamicSolver
{
    double[] _pole = new double[] { 0.05, 0.3, 0.05, 0.3, 0.05 };
    (double min, double max)[] _limit = new (double min, double max)[] { (-Math.PI * 2, Math.PI * 2), (-Math.PI / 2, Math.PI / 2), (0, Math.PI), (-Math.PI * 2, Math.PI * 2), (-Math.PI / 2, Math.PI / 2), (-Math.PI * 2, Math.PI * 2) };
    double angle2Offset = 0.0;
    public SixAxis2025InverseDynamic((double mim, double max)[]? limits = null, double[]? pole = null)
    {
        _pole = pole ?? _pole;
        _limit = limits ?? _limit;
        angle2Offset = -Math.Atan2(_pole[2], _pole[3]);
    }
    private bool SolveTail(double angle1, double angle2, double angle0, (Vector3d position, Quaterniond forward) target, ref List<double[]> thetas)
    {
        var ret = false;
        var tailQ = Quaterniond.AxisAngleR(Vector3d.AxisZ, angle0) * Quaterniond.AxisAngleR(Vector3d.AxisY, angle1 + angle2);
        var tailPitchAxisZ = tailQ * Vector3d.AxisZ;
        var targetAxisX = target.forward * Vector3d.AxisX;
        var tailPitchAxisY = Vector3d.Cross(tailPitchAxisZ, targetAxisX).Normalized;
        var tailRollAxisY = tailQ * Vector3d.AxisY;
        var tailRollAxisX = tailQ * Vector3d.AxisX;
        var rollAngle1 = Math.Atan2(-tailPitchAxisY.Dot(tailRollAxisX.Normalized), tailPitchAxisY.Dot(tailRollAxisY.Normalized));
        var pitchAngle = Vector3d.AngleR(tailPitchAxisZ, targetAxisX);

        var targetNormal = target.forward * Vector3d.AxisZ;
        var targetUp = target.forward * Vector3d.AxisY;
        var tailRollAngle = Math.Atan2(tailPitchAxisY.Dot(targetUp), tailPitchAxisY.Dot(targetNormal));
        if (angle1 > _limit[1].min && angle1 < _limit[1].max && angle2 > _limit[2].min && angle2 < _limit[2].max && rollAngle1 > _limit[3].min && rollAngle1 < _limit[3].max)
        {
            if (pitchAngle > _limit[4].min && pitchAngle < _limit[4].max)
            {
                thetas.Add(new double[] { angle0, angle1, angle2, RescaleToPi(rollAngle1), pitchAngle, RescaleToPi(tailRollAngle) });
                ret = true;
            }

            if (-pitchAngle > _limit[4].min && -pitchAngle < _limit[4].max)
            {
                thetas.Add(new double[] { angle0, angle1, angle2, RescaleToPi(rollAngle1 + Math.PI), -pitchAngle, RescaleToPi(tailRollAngle - Math.PI) });
                ret = true;
            }
        }
        return ret;
    }
    double RescaleToPi(double rad, double[]? limit = null)
    {
        if (limit == null)
        {
            while (rad > Math.PI)
                rad -= 2 * Math.PI;
            while (rad < -Math.PI)
                rad += 2 * Math.PI;
            return rad;
        }
        while (rad > limit[1])
            rad -= 2 * Math.PI;
        while (rad < limit[0])
            rad += 2 * Math.PI;
        return rad;
    }
    public bool Solve((Vector3d position, Quaterniond forward) target, out List<double[]> thetas)
    {
        thetas = new();
        bool ret = false;

        var endYawPosition = target.position - target.forward * Vector3d.AxisX * _pole[^1] - _pole[0] * Vector3d.AxisZ;
        var _d1 = _pole[1];
        var _d2 = Math.Sqrt(_pole[2] * _pole[2] + _pole[3] * _pole[3]);
        var angle0 = Math.Atan2(endYawPosition.y, endYawPosition.x);
        var angle2 = Math.PI - Math.Acos(-(endYawPosition.LengthSquared - _d1 * _d1 - _d2 * _d2) / (2 * _d1 * _d2)) + angle2Offset;
        var angle11 = Math.Atan2(endYawPosition.xy.Length, endYawPosition.z);
        var angle12 = Math.Acos(-(_d2 * _d2 - endYawPosition.LengthSquared - _d1 * _d1) / (2 * endYawPosition.Length * _d1));

        ret = SolveTail(RescaleToPi(angle11 + angle12, new double[] { _limit[1].min, _limit[1].max }), -angle2, angle0, target, ref thetas) || ret;
        ret = SolveTail(RescaleToPi(angle11 - angle12, new double[] { _limit[1].min, _limit[1].max }), angle2, angle0, target, ref thetas) || ret;

        return ret;
    }

}
