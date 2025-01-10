using AutoExchange.ExchangeStationDetector;
using AutoExchange.ExchangeStationDetector.Old;
using Compunet.YoloSharp.Data;
using Emgu.CV.PpfMatch3d;
using RapidlyArmPlanner.ArmSolver.ForwardDynamic;
using RapidlyArmPlanner.ArmSolver.InverseDynamic;
using RapidlyArmPlanner.ColliderDetector;
using RapidlyArmPlanner.PathFinder.RRT_BHAStar;
using TlarcKernel.IO.ROS2Msgs.Std;

namespace Engineer.Arm;

class SixAxis : Component
{
    (Vector3d position, Quaterniond rotation) cameraInBase = (new(0.04915,-0.03573,0.09735), Quaterniond.AxisAngleD(Quaterniond.AxisAngleD(Vector3d.AxisY,45)*Vector3d.AxisZ,-90) * Quaterniond.AxisAngleD(Vector3d.AxisY,45));
    readonly double[] _pole = [0.05985, 0.41, 0.08307, 0.33969, 0.0571];
    AutoExchange.ArmPlanner planner;
    YoloRedemptionDetector redemptionDetector;
    public DateTime constructTime = DateTime.MinValue;
    double[] joints;
    List<RapidlyArmPlanner.TrajectoryFit.BSplineTrajectoryWithMinimalSnap> trajectory;
    Bool beginSub;
    bool begin = false;
    FloatMultiArray floatMultiArray;
    IO.ROS2Msgs.Geometry.PoseStampd pose3D;
    public override void Start()
    {
        var forwardDynamic = new SixAxis2025ForwardDynamic(pole: _pole);
        var inverseDynamicSolver = new SixAxis2025InverseDynamic(pole: _pole);
        var colliderDetector = new SixAxisRedemptionDetector();
        var searcher = new PathToPathLoose(
            new RRT_BHAStar(
                [Math.PI,1.3 , 1.2 + Math.PI/2, Math.PI, Math.PI/2,Math.PI],
                [-Math.PI, -1.2, -0.8727+Math.PI/2, -Math.PI, -Math.PI/2, -Math.PI])
            {
                forwardDynamic = forwardDynamic,
                obstacleDetector = colliderDetector
            }
        );
        beginSub = new(IOManager);
        floatMultiArray = new(IOManager);
        pose3D = new(IOManager);
        pose3D.RegistryPublisher("/redemption/position");
        beginSub.Subscript("/engineer/exchange", x => {begin = x;trajectory = [];});
        joints = new double[6];
        floatMultiArray.RegistryPublisher("/engineer/joint/control");
        floatMultiArray.Subscript("/engineer/joint/measure", x => { for (int i = 0; i < 6; i++) joints[i] = x[i]; });

        planner = new() { forwardDynamic = forwardDynamic, inverseDynamicSolver = inverseDynamicSolver, colliderDetector = colliderDetector, searcher = searcher };
    }
    public override void Update()
    {
        var isSuccess = trajectory is not null && trajectory.Count  != 0;
        if (!begin)
            return;
        if (!isSuccess )
        {
            var (position, rotation) = redemptionDetector.redemptionInCamera;

            isSuccess = planner.PlanTrajectory(joints, (Quaterniond.AxisAngleR(Vector3d.AxisZ, joints[0]) * (cameraInBase.position + cameraInBase.rotation * position), Quaterniond.AxisAngleR(Vector3d.AxisZ, joints[0]) * cameraInBase.rotation * rotation), out var tmpTrajectory);
            trajectory = tmpTrajectory;
            constructTime = DateTime.Now;
        }
      if (!isSuccess){
        begin = false;
            return;}
        begin = !((DateTime.Now - constructTime).TotalSeconds >= trajectory.Max(x => x.MaxTime));

        floatMultiArray.Publish(trajectory.Select(x => (float)x.GetPosition((DateTime.Now - constructTime).TotalSeconds)).ToArray());
    }
}
