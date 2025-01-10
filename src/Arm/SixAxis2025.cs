using AutoExchange.ExchangeStationDetector;
using AutoExchange.ExchangeStationDetector.Old;
using Compunet.YoloSharp.Data;
using RapidlyArmPlanner.ArmSolver.ForwardDynamic;
using RapidlyArmPlanner.ArmSolver.InverseDynamic;
using RapidlyArmPlanner.ColliderDetector;
using RapidlyArmPlanner.PathFinder.RRT_BHAStar;
using TlarcKernel.IO.ROS2Msgs.Std;

namespace Engineer.Arm;

class SixAxis : Component
{
    (Vector3d position, Quaterniond rotation) cameraInBase;
    readonly double[] _pole = [0.05, 0.3, 0.05, 0.3, 0.05];
    AutoExchange.ArmPlanner planner;
    YoloRedemptionDetector redemptionDetector;
    public DateTime constructTime = DateTime.MinValue;
    double[] joints;
    List<RapidlyArmPlanner.TrajectoryFit.BSplineTrajectoryWithMinimalSnap> trajectory;
    Bool beginSub;
    bool begin = false;
    FloatMultiArray floatMultiArray;
    public override void Start()
    {
        var forwardDynamic = new SixAxis2025ForwardDynamic(pole: _pole);
        var inverseDynamicSolver = new SixAxis2025InverseDynamic(pole: _pole);
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
        joints = new double[6];
        floatMultiArray.RegistryPublisher("/engineer/joint/control");
        floatMultiArray.Subscript("/engineer/joint/measure", x => { for (int i = 0; i < 6; i++) joints[i] = x[i]; });

        planner = new() { forwardDynamic = forwardDynamic, inverseDynamicSolver = inverseDynamicSolver, colliderDetector = colliderDetector, searcher = searcher };
    }
    public override void Update()
    {
        var isSuccess = trajectory != null;
        if (!begin)
            return;
        if (!isSuccess || (DateTime.Now - constructTime).TotalSeconds >= trajectory.Max(x => x.MaxTime))
        {
            var redemptionInCamera = redemptionDetector.redemptionInCamera;

            isSuccess = planner.PlanTrajectory(joints, (cameraInBase.position + redemptionInCamera.rotation * redemptionInCamera.position, cameraInBase.rotation * redemptionInCamera.rotation), out var tmpTrajectory);
            trajectory = tmpTrajectory;
            constructTime = DateTime.Now;
        }

        begin = !((DateTime.Now - constructTime).TotalSeconds >= trajectory.Max(x => x.MaxTime));
        if (!isSuccess)
            return;

        floatMultiArray.Publish((float[])trajectory.Select(x => (float)x.GetPosition((DateTime.Now - constructTime).TotalSeconds)));
    }
}
