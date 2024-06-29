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

    public float limitVelocity = 5 + 1e-4f;
    public float limitAccelerate = 2f + 1e-4f;
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
    private Transform2D target;

    public List<Vector3> Path => _path;
    public Vector3 TargetVelocity => nonUniformBSpline.GetVelocity((DateTime.Now.Ticks - _timeTicks) / TimeSpan.TicksPerSecond);
    public Vector3 TargetAccelerate => nonUniformBSpline.GetAcceleration((DateTime.Now.Ticks - _timeTicks) / TimeSpan.TicksPerSecond);

    private NonUniformBSpline nonUniformBSpline;
    private List<Vector3> _path;

    private Vector2 _beginSpeed;
    private Vector2 _targetPoint;

    private IO.ROS2Msgs.Geometry.Pose2D _beginSpeedReceiver;
    private IO.ROS2Msgs.Nav.Path _pathPublisher;

    private float _timeTicks;

    public override void Start()
    {
        _beginSpeedReceiver = new();
        _beginSpeedReceiver.Subscript(beginSpeedTopicName, data => { _beginSpeed = data.pos; });
        _pathPublisher = new();
        _pathPublisher.RegistryPublisher(pathTopicName);
        _targetPoint = new(100, 100);
        nonUniformBSpline = new NonUniformBSpline(limitVelocity, limitAccelerate, limitRatio);
        _timeTicks = DateTime.Now.Ticks;
    }

    public override void Update()
    {
        if (target.position != _targetPoint) { Build(); return; }

        var (id, isSafe) = nonUniformBSpline.Check(
            (DateTime.Now.Ticks - _timeTicks) / TimeSpan.TicksPerSecond, costMap, sentry.position);

        if (!isSafe)
            Build();
    }

    private void Build()
    {
        _timeTicks = DateTime.Now.Ticks;
        nonUniformBSpline.ParametersToControlPoints([.. hybridAStar.Path.SkipLast(1), .. dijkstra.Path], [_beginSpeed, new(0, 0)]);
        nonUniformBSpline.BuildTimeLine(timeInterval);
#if DEBUG
        _path = nonUniformBSpline.GetPath();
        _pathPublisher.Publish([.. _path]);
#endif
    }

}