namespace ALPlanner.PathPlanner.Nodes;

class AccelerateNode : INode
{
  const double Accelerate1 = 1f;
  const double Accelerate2 = 0.3f;
  const double timeStep = PathPlanner.TimeStep;

  double _g = 0;
  public double _h = 0;

  public static INode? Target { get; set; }

  public AccelerateNode(Vector3d position, Vector3d speed)
  {
    PositionInWorld = position;
    SpeedInWorld = speed;
  }

  public AccelerateNode(Vector3d position, AccelerateNode? parent)
  {
    PositionInWorld = position;
    SpeedInWorld = (position - parent.PositionInWorld).Normalized;
    Parent = parent;
    _g = parent._g + 1;
    _h = SpeedInWorld.Length;
  }

  public Vector3d PositionInWorld { get; private set; }
  public Vector3d SpeedInWorld { get; private set; }

  public IEnumerable<INode> Children
  {
    get
    {
      var speedDir1 = SpeedInWorld.Normalized * Accelerate1;
      var crossSpeedDir1 = new Vector3d(-SpeedInWorld.y, SpeedInWorld.x, SpeedInWorld.z).Normalized * Accelerate1;
      var speedDir2 = SpeedInWorld.Normalized * Accelerate2;
      var crossSpeedDir2 = new Vector3d(-SpeedInWorld.y, SpeedInWorld.x, SpeedInWorld.z).Normalized * Accelerate2;
      List<INode> children = [
        new AccelerateNode(PositionInWorld + SpeedInWorld * timeStep + speedDir1 * timeStep * timeStep / 2, SpeedInWorld + speedDir1){Parent = this} ,
        new AccelerateNode(PositionInWorld + SpeedInWorld * timeStep - speedDir1 * timeStep * timeStep / 2, SpeedInWorld - speedDir1){Parent = this},
        new AccelerateNode(PositionInWorld + SpeedInWorld * timeStep + crossSpeedDir1 * timeStep * timeStep / 2, SpeedInWorld + crossSpeedDir1){Parent = this},
        new AccelerateNode(PositionInWorld + SpeedInWorld * timeStep - crossSpeedDir1 * timeStep * timeStep / 2, SpeedInWorld - crossSpeedDir1){Parent = this},
       ];
      if (SpeedInWorld.Length != 0)
        children.Add(
                new AccelerateNode(PositionInWorld + SpeedInWorld * timeStep, SpeedInWorld) { Parent = this });
      return children;
    }
  }

  public INode? Parent { get; set; } = null;

  public float TotalCost => (float)(_g + _h);

  public bool GeometricallyEqualTo(INode node)
  {
    return (node.PositionInWorld - PositionInWorld).Length < timeStep * 1;
  }
}
