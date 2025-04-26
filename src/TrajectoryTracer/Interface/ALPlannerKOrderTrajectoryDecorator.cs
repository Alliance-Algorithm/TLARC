
using ALPlanner.TrajectoryOptimizer;
using TlarcKernel.IO.ProcessCommunicateInterfaces;
using TlarcKernel.TrajectoryOptimizer.Curves;

namespace TrajectoryTracer.Trajectory;

class ALPlannerKOrderTrajectoryDecorator : Component, ITrajectory<Vector3d>
{
    [ComponentReferenceFiled]
    IKOrderCurve kOrderCurve;

    IO.ROS2Msgs.Nav.Path debugPath;
    Vector3d[] trajectory = [];
    public override void Start()
    {
#if DEBUG
        debugPath = new(IOManager);
        debugPath.RegistryPublisher("debug/mpc/trajectory");
#endif
    }
    public override void Update()
    {
#if DEBUG
        debugPath.Publish(trajectory);
#endif
    }
    public Vector3d[] Trajectory(double howLong, int count)
    {
        var time = (DateTime.Now - kOrderCurve.ConstructTime).TotalSeconds;
        trajectory = kOrderCurve.TrajectoryPoints(time, time + howLong, howLong / count).ToArray();
        return trajectory;
    }
    public Vector3d[] Velocities(double howLong, int count)
    {
        var time = (DateTime.Now - kOrderCurve.ConstructTime).TotalSeconds;
        return kOrderCurve.VelocitiesPoints(time, time + howLong, howLong / count).ToArray();
    }
}