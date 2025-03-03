
using ALPlanner.TrajectoryOptimizer;
using TlarcKernel.IO.ProcessCommunicateInterfaces;
using TlarcKernel.TrajectoryOptimizer.Curves;

namespace TrajectoryTracer.Trajectory;

class ALPlannerKOrderTrajectoryDecorator : Component, ITrajectory<Vector3d>
{
    [ComponentReferenceFiled]
    IKOrderCurve kOrderCurve;

    IO.ROS2Msgs.Nav.Path debugPath;
    public override void Start()
    {
#if TLRAC_DEBUG
        debugPath = new(IOManager);
        debugPath.RegistryPublisher("debug/mpc/trajectory");
#endif
    }
    public override void Update()
    {
        var trajectory = Trajectory(2, 20);

#if TLRAC_DEBUG
        debugPath.Publish(trajectory);
#endif
    }
    public Vector3d[] Trajectory(double howLong, int count)
    {
        var time = (DateTime.Now - kOrderCurve.ConstructTime).TotalSeconds;
        return kOrderCurve.TrajectoryPoints(time, time + howLong, howLong / count).ToArray();
    }
}