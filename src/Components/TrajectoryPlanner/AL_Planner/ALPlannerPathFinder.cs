using System.Diagnostics;
using System.Numerics;
using Tlarc.IO.ROS2Msgs.Nav;
using Tlarc.StdComponent;
using Rosidl.Messages.Geometry;

using Vector3 = System.Numerics.Vector3;
using Tlarc.TrajectoryPlanner.Utils;

namespace Tlarc.ALPlanner.Trajectory;

class ALPathFinder : Component
{
    public string beginSpeedTopicName = "/chassis/speed";
    public string pathTopicName = "/chassis/path";
    public string bSplineControlMatrixTopicName = "/chassis/trajectory/b_spline/control_matrix";

    public float limitVelocity = 5.0f + 1e-4f;
    public float limitAccelerate = 1.5f + 1e-4f;
    public float limitRatio = 4f + 1e-4f;
    public float lambda1 = 0f;
    public float lambda2 = 1f;
    public float lambda3 = 0;
    public int order = 4;
    public float timeInterval = 0.5f;

    private ESDFMap costMap;
    private JumpPointMap jumpMap;
    private HybridAStar hybridAStar;
    private Transform2D sentry;
    private ALPlannerDecisionMaker decisionMaker;

    public List<Vector3> Path { get; private set; }

    private NonUniformBSpline nonUniformBSpline;
    private Vector2 _beginSpeed;
    private Vector2 _lastPosition;
    private Vector2 _targetPoint;

    private IO.ROS2Msgs.Geometry.Pose2D _beginSpeedReceiver;
    private IO.ROS2Msgs.Nav.Path _pathPublisher;
    private IO.ROS2Msgs.Nav.Path _hybridAStarPathPublisher;
    private IO.ROS2Msgs.Nav.Path _bSplinePublisher;

    public Vector3 TargetVelocity => nonUniformBSpline.GetVelocity((_timeTicksTarget - _timeTicks) / 1e7f);
    public Vector3 TargetAccelerate => nonUniformBSpline.GetAcceleration((_timeTicksTarget - _timeTicks) / 1e7f);
    public Vector2 TargetPosition
    {
        get
        {
            var pos = nonUniformBSpline.GetPosition((_timeTicksTarget - _timeTicks) / 1e7f);
            return float.IsNaN(pos.X) ? sentry.Position : pos;
        }
    }
    public double TargetKesi(float deltaT)
    {
        var tmp = TargetVelocity;
        var tmp2 = nonUniformBSpline.GetVelocity((_timeTicksTarget - _timeTicks) / 1e7f + deltaT);
        var cos_kesi = Vector3.Dot(tmp, tmp2) / tmp.Length() / tmp2.Length();
        return Math.Acos(cos_kesi) * Math.Sign(Vector3.Cross(tmp, tmp2).Z);
    }


    private long _timeTicks, _timeTicksTarget;

    public override void Start()
    {
        _beginSpeedReceiver = new(IOManager);
        _beginSpeedReceiver.Subscript(beginSpeedTopicName, data => { _beginSpeed = data.pos; if (_beginSpeed.Length() < 0.1f) _beginSpeed = Vector2.Zero; });
        _pathPublisher = new(IOManager);
        _pathPublisher.RegistryPublisher(pathTopicName + "/Trajectory");
        _hybridAStarPathPublisher = new(IOManager);
        _hybridAStarPathPublisher.RegistryPublisher(pathTopicName + "/AStar");
        _bSplinePublisher = new(IOManager);
        _bSplinePublisher.RegistryPublisher(bSplineControlMatrixTopicName);
        _targetPoint = new(100, 100);
        nonUniformBSpline = new NonUniformBSpline(limitVelocity, limitAccelerate, limitRatio);
        _timeTicks = DateTime.Now.Ticks;
    }


    public override void Update()
    {

    }

    private void Build()
    {

    }

}