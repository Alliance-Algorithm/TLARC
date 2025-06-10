namespace ALPlanner.PathPlanner.Nodes;

class Omni2DConsecNode : INode
{
  const double IterationStep = 0.1f;
  const int Headings = 4;

  double _g = 0;
  double _h = 0;

  public static INode? Target { get; set; }

  public Omni2DConsecNode(Vector3d position)
  {
    PositionInWorld = position;
  }

  public Omni2DConsecNode(Vector3d position, Omni2DConsecNode? parent)
  {
    PositionInWorld = position;
    Parent = parent;
    _g = parent._g + IterationStep;
    _h = (Target.PositionInWorld - PositionInWorld).Length;
  }

  public Vector3d PositionInWorld { get; private set; }

  public IEnumerable<INode> Children
  {
    get
    {
      List<INode> children = new();
      for (int i = 0; i < Headings; i++)
      {
        var (Sin, Cos) = Math.SinCos(i * Math.Tau / Headings);
        children.Add(
          new Omni2DConsecNode(PositionInWorld + IterationStep * new Vector3d(Cos, Sin, 0), this)
        );
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
