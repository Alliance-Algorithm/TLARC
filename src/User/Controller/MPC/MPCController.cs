using System.Numerics;
using AllianceDM.StdComponent;
using MathNet.Numerics.LinearAlgebra.Double;

namespace AllianceDM.Controller.MPC;

class MPCController : Component
{
    private ALPlanner.ALPathFinder pathFinder;
    private Transform2D sentry;
    private Vector2 _lastPos;
    private DenseVector _u;
    AllianceDM.IO.ROS2Msgs.Geometry.Pose2D velocityPub;
    private double lastAngle = 0;
    private double lastKesiAngle = 0;
    private float t = 0.05f;
    private float l = 0.15f;
    MPCCalculator calculator;

    public override void Start()
    {
        calculator = new(3, 2, 10);
        calculator.Init();
        _u = new DenseVector(2);
        velocityPub = new();
        velocityPub.RegistryPublisher("/sentry/control/velocity");
    }

    override public void Update()
    {
        var targetPos = pathFinder.TargetPosition;
        if (targetPos == sentry.position)
        {
            calculator.Reset();
        }
        var targetVel = pathFinder.TargetVelocity;
        var kesi = pathFinder.TargetKesi(t);

        var a = new DenseMatrix(3, 3);
        var b = new DenseMatrix(3, 2);
        a[0, 0] = 1;
        a[1, 1] = 1;
        a[2, 2] = 1;
        a[0, 2] = t * -targetVel.Y;
        a[1, 2] = t * targetVel.X;
        var tempAngle = lastAngle;
        if (targetVel.Length() != 0)
            tempAngle = Math.Atan2(targetVel.Y, targetVel.X);
        b[0, 0] = t * Math.Cos(tempAngle);
        b[1, 0] = t * Math.Sin(tempAngle);
        if (!double.IsNaN(kesi))
            lastKesiAngle = kesi;
        b[2, 0] = t * Math.Tan(lastKesiAngle) / l;
        b[2, 1] = t * targetVel.Length() / l / Math.Pow(Math.Cos(lastKesiAngle), 2);

        DenseVector xRef = new DenseVector(3);
        xRef[0] = targetPos.X;
        xRef[1] = targetPos.Y;
        xRef[2] = Math.Atan2(targetVel.Y, targetVel.X);
        DenseVector x = new DenseVector(3);
        x[0] = sentry.position.X;
        x[1] = sentry.position.Y;

        x[2] = Math.Atan2(sentry.position.Y - _lastPos.Y, sentry.position.X - _lastPos.X); ;
        DenseMatrix uMin = new DenseMatrix(1, 2);
        DenseMatrix uMax = new DenseMatrix(1, 2);
        uMin[0, 0] = -10;
        uMin[0, 1] = -1;
        uMax[0, 0] = 10;
        uMax[0, 1] = 1;
        DenseMatrix deltaUMin = new DenseMatrix(1, 2);
        DenseMatrix deltaUMax = new DenseMatrix(1, 2);
        deltaUMin[0, 0] = -2;
        deltaUMin[0, 1] = -0.2;
        deltaUMax[0, 0] = 2;
        deltaUMax[0, 1] = 0.2;
        // _u = calculator.Calculate(a, b, x, xRef, uMin, uMax, deltaUMin, deltaUMax);
        // var vel = new Vector2((float)((_u[0] + targetVel.Length()) * Math.Cos(_u[1] + tempAngle - sentry.angle)), (float)((_u[0] + targetVel.Length()) * Math.Sin(_u[1] + tempAngle - sentry.angle)));
        var vel = (targetPos - sentry.position) / 0.1f;
        lastAngle = tempAngle;
        velocityPub.Publish((vel / vel.Length() * Math.Clamp(vel.Length(), 0, 5), (float)_u[1]));
        _lastPos = sentry.position;
    }

}