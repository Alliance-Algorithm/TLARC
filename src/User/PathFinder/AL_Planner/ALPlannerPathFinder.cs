using System.Numerics;
using AllianceDM.IO.ROS2Msgs.Nav;
using AllianceDM.StdComponent;
using Rosidl.Messages.Geometry;

using Vector3 = System.Numerics.Vector3;

namespace AllianceDM.ALPlanner;

class ALPathFinder : Component
{
    public string beginSpeedTopicName = "/chassis/speed";
    public string pathTopicName = "/chassis/path";
    public string bSplineControlMatrixTopicName = "/chassis/trajectory/b_spline/control_matrix";

    public float limitVelocity = 5 + 1e-4f;
    public float limitAccelerate = 1f + 1e-4f;
    public float limitRatio = 4f + 1e-4f;
    public float lambda1 = 0f;
    public float lambda2 = 1f;
    public float lambda3 = 0;
    public int order = 4;
    public float timeInterval = 0.5f;

    private GlobalESDFMap costMap;
    private Dijkstra dijkstra;
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
    private IO.ROS2Msgs.Nav.Path _bSplinePublisher;

    public Vector3 TargetVelocity(float t) => nonUniformBSpline.GetVelocity((DateTime.Now.Ticks - _timeTicks) / 1e7f + t);
    public Vector3 TargetAccelerate(float t) => nonUniformBSpline.GetAcceleration((DateTime.Now.Ticks - _timeTicks) / 1e7f + t);
    public Vector2 TargetPosition(float t)
    {
        var pos = nonUniformBSpline.GetPosition((DateTime.Now.Ticks - _timeTicks) / 1e7f + t);
        return float.IsNaN(pos.X) ? sentry.position : pos;
    }
    public double TargetKesi(float t, float deltaT)
    {
        var tmp = TargetVelocity(t);
        var tmp2 = nonUniformBSpline.GetVelocity((DateTime.Now.Ticks - _timeTicks) / 1e7f + t + deltaT);
        var cos_kesi = Vector3.Dot(tmp, tmp2) / tmp.Length() / tmp2.Length();
        return Math.Acos(cos_kesi) * Math.Sign(Vector3.Cross(tmp, tmp2).Z);
    }


    private long _timeTicks;

    public override void Start()
    {
        _beginSpeedReceiver = new();
        _beginSpeedReceiver.Subscript(beginSpeedTopicName, data => { _beginSpeed = data.pos; if (_beginSpeed.Length() < 0.1f) _beginSpeed = Vector2.Zero; });
        _pathPublisher = new();
        _pathPublisher.RegistryPublisher(pathTopicName);
        _bSplinePublisher = new();
        _bSplinePublisher.RegistryPublisher(bSplineControlMatrixTopicName);
        _targetPoint = new(100, 100);
        nonUniformBSpline = new NonUniformBSpline(limitVelocity, limitAccelerate, limitRatio);
        _timeTicks = DateTime.Now.Ticks;
    }


    public override void Update()
    {
        _beginSpeed = (sentry.position - _lastPosition) * DecisionMakerDef.fps / 1000.0f;
        _lastPosition = sentry.position;
        if (decisionMaker.TargetPosition != _targetPoint)
        {
            Build();
            _targetPoint = decisionMaker.TargetPosition;
            return;
        }

        var (id, isSafe) = nonUniformBSpline.Check(
            (DateTime.Now.Ticks - _timeTicks) / 1e7f, costMap, sentry.position);

        if (!isSafe)
            Build();
    }

    private void Build()
    {
        _timeTicks = DateTime.Now.Ticks;
        nonUniformBSpline.ParametersToControlPoints([.. hybridAStar.Path.SkipLast(1), .. dijkstra.Path.Skip(hybridAStar.Path.Count > 1 ? 1 : 0)], [_beginSpeed, new(0, 0)]);
        nonUniformBSpline.BuildTimeLine(timeInterval);
        var controlMatrix = nonUniformBSpline._controlPoints;
        var timeControlMatrix = nonUniformBSpline._timeControlPoints;
        List<Vector3> data = new();
        for (int i = 0; i < controlMatrix.RowCount; i++)
        {
            data.Add(new(controlMatrix[i, 0], controlMatrix[i, 1], i < timeControlMatrix.RowCount ? timeControlMatrix[i, 0] : 0));
        }
        _bSplinePublisher.Publish([.. data]);

#if DEBUG
        Path = nonUniformBSpline.GetPath();
        // _pathPublisher.Publish([.. Path]);
#endif

    }

}