namespace RapidlyArmPlanner.ArmSolver.InverseDynamic;
class Scara2025InverseDynamic : IInverseDynamicSolver
{
    public Scara2025InverseDynamic(double d1, double d2, double d3, (double mim, double max) limit1, (double mim, double max) limit2, (double mim, double max) limit3)
    {
        _d1 = d1;
        _d2 = d2;
        _d3 = d3;
        _l1 = limit1;
        _l2 = limit2;
        _l3 = limit3;
    }
    private readonly double _d1, _d2, _d3;
    private readonly (double min, double max) _l1, _l2, _l3;
    private bool SolveTail(double angle1, double angle2, double height, (Vector3d position, Quaterniond forward) target, ref List<double[]> thetas)
    {
        var ret = false;
        var tailForward = Quaterniond.AxisAngleR(Vector3d.AxisZ, angle1 + angle2) * Vector3d.AxisX;
        var targetForward = target.forward * Vector3d.AxisX;
        var tailNormal = Vector3d.Cross(tailForward, targetForward).Normalized;
        var tailUp = Vector3d.Cross(Vector3d.AxisZ, tailForward).Normalized;
        var rollAngle = Math.Atan2(tailUp.Dot(tailNormal), tailNormal.z);
        var yawAngle = Vector3d.AngleR(tailForward, targetForward);

        var targetNormal = target.forward * Vector3d.AxisZ;
        var targetUp = target.forward * Vector3d.AxisY;
        tailUp = Vector3d.Cross(targetNormal, targetForward).Normalized;
        var tailRollAngle = Math.Atan2(tailNormal.Dot(targetUp), tailNormal.Dot(targetNormal));
        if (angle1 > _l1.min && angle1 < _l1.max && angle2 > _l2.min && angle2 < _l2.max && height > 0)
        {
            if (yawAngle > _l3.min && yawAngle < _l3.max)
            {
                thetas.Add([angle1, height, angle2, RescaleToPi(-rollAngle), yawAngle, RescaleToPi(tailRollAngle)]);
                ret = true;
            }
            if (-yawAngle > _l3.min && -yawAngle < _l3.max)
            {
                thetas.Add([angle1, height, angle2, RescaleToPi(-rollAngle + Math.PI), -yawAngle, RescaleToPi(tailRollAngle - Math.PI)]);
                ret = true;
            }
        }
        return ret;
    }
    static double RescaleToPi(double rad, double[]? limit = null)
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
        thetas = [];
        bool ret = false;

        var endYawPosition = target.position - target.forward * Vector3d.AxisX * _d3;
        var height = endYawPosition.z;
        endYawPosition.z = 0;
        var angle2 = Math.PI - Math.Acos(-(endYawPosition.LengthSquared - _d1 * _d1 - _d2 * _d2) / (2 * _d1 * _d2));
        var angle11 = Math.Atan2(endYawPosition.y, endYawPosition.x);
        var angle12 = Math.Acos(-(_d2 * _d2 - endYawPosition.LengthSquared - _d1 * _d1) / (2 * endYawPosition.Length * _d1));

        ret = SolveTail(RescaleToPi(angle11 + angle12, [_l1.min, _l1.max]), -angle2, height, target, ref thetas) || ret;
        ret = SolveTail(RescaleToPi(angle11 - angle12, [_l1.min, _l1.max]), angle2, height, target, ref thetas) || ret;

        return ret;
    }

}
