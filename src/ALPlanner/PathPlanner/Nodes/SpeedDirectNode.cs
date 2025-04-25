namespace ALPlanner.PathPlanner.Nodes;

class SpeedDirectNode : INode
{
    const double IterationStep = 0.15f;
    const int Headings = 8;

    double _g = 0;
    double _h = 0;

    double AngleRad = Math.PI / 2;


    public static INode? Target { get; set; }

    public SpeedDirectNode(Vector3d position, Vector3d speed)
    {
        PositionInWorld = position;
        SpeedInWorld = speed;
    }
    public SpeedDirectNode(Vector3d position, SpeedDirectNode? parent)
    {
        PositionInWorld = position;
        SpeedInWorld = (position - parent.PositionInWorld).Normalized;
        Parent = parent;
        _g = parent._g + IterationStep;
        _h = (Target.PositionInWorld - PositionInWorld).Length;
    }
    public Vector3d PositionInWorld { get; private set; }
    public Vector3d SpeedInWorld { get; private set; }

    public IEnumerable<INode> Children
    {
        get
        {
            List<INode> children = new();
            for (int i = 0; i < Headings; i++)
            {
                if (SpeedInWorld.LengthSquared == 0)
                {
                    var speedAngle = Math.Atan2(SpeedInWorld.y, SpeedInWorld.x);
                    var sinCos = Math.SinCos(i * Math.Tau / Headings);
                    children.Add(new SpeedDirectNode(PositionInWorld + IterationStep * new Vector3d(sinCos.Cos, sinCos.Sin, 0), this));
                }
                else
                {
                    var speedAngle = Math.Atan2(SpeedInWorld.y, SpeedInWorld.x);
                    var sinCos = Math.SinCos(speedAngle + i * AngleRad / Headings - AngleRad / 2);
                    children.Add(new SpeedDirectNode(PositionInWorld + IterationStep * new Vector3d(sinCos.Cos, sinCos.Sin, 0), this));
                }
            }
            return children;
        }
    }

    public INode? Parent { get; set; } = null;

    public float TotalCost => (float)(_g + _h);

    public bool GeometricallyEqualTo(INode node)
    {
        return (node.PositionInWorld - PositionInWorld).Length < IterationStep;
    }
}