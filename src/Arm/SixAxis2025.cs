using AutoExchange.ExchangeStationDetector;
using AutoExchange.ExchangeStationDetector.Old;
using RapidlyArmPlanner.ArmSolver.ForwardDynamic;
using RapidlyArmPlanner.ArmSolver.InverseDynamic;
using RapidlyArmPlanner.ColliderDetector;
using RapidlyArmPlanner.PathFinder.RRT_BHAStar;
using TlarcKernel.IO.ROS2Msgs.Std;

namespace Engineer.Arm;

class SixAxis : Component
{
    AutoExchange.ArmPlanner planner;
    YoloRedemptionDetector redemptionDetector;
    public DateTime constructTime = DateTime.MinValue;
    Joint[] joints;
    List<RapidlyArmPlanner.TrajectoryFit.BSplineTrajectoryWithMinimalSnap> trajectory;
    Bool beginSub;
    bool begin = false;
    FloatMultiArray floatMultiArray;
    public override void Start()
    {
        var forwardDynamic = new SixAxis2025ForwardDynamic();
        var inverseDynamicSolver = new SixAxis2025InverseDynamic();
        var colliderDetector = new SixAxisRedemptionDetector();
        var searcher = new PathToPathLoose(
            new RRT_BHAStar(
                [Math.PI, 0.8, Math.PI, Math.PI, Math.PI, Math.PI],
                [-Math.PI, 0, -Math.PI, -Math.PI, -Math.PI, -Math.PI])
            {
                forwardDynamic = forwardDynamic,
                obstacleDetector = colliderDetector
            }
        );
        beginSub = new(IOManager);
        floatMultiArray = new(IOManager);
        beginSub.Subscript("/engineer/exchange", x => begin = x);
        floatMultiArray.RegistryPublisher("/engineer/joint_target");
        joints = [new("Yaw1", IOManager), new("Pitch1", IOManager), new("Pitch2", IOManager), new("Roll1", IOManager), new("Pitch3", IOManager), new("Roll2", IOManager)];

        planner = new() { forwardDynamic = forwardDynamic, inverseDynamicSolver = inverseDynamicSolver, colliderDetector = colliderDetector, searcher = searcher };
    }
    public override void Update()
    {
        var isSuccess = trajectory != null;
        if (!begin)
            return;
        if (!isSuccess || (DateTime.Now - constructTime).TotalSeconds >= trajectory.Max(x => x.MaxTime))
        {
            isSuccess = planner.PlanTrajectory(joints.GetValueArray(), redemptionDetector.Translate, out var tmpTrajectory);
            trajectory = tmpTrajectory;
            constructTime = DateTime.Now;
        }

        begin = !((DateTime.Now - constructTime).TotalSeconds >= trajectory.Max(x => x.MaxTime));
        if (!isSuccess)
            return;

        floatMultiArray.Publish((float[])trajectory.Select(x => (float)x.GetPosition((DateTime.Now - constructTime).TotalSeconds)));
    }
}
