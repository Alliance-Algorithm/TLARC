using Accord.Math;

class RRTStarNode
{
  double[] _values;

  public double[] Values => _values;

  double _cost = 0;
  public double Cost => _cost;

  RRTStarNode? _parent = null;
  public RRTStarNode? Parent => _parent;

  public RRTStarNode(double[] values, double cost = 0, RRTStarNode? parent = null)
  {
    _values = values;
    _cost = cost;

    _parent = parent ?? this;
  }

  const double step = 0.2;

  public double CalculateDistance(double[] node) => _values.Subtract(node).SquareEuclidean();

  public RRTStarNode GenerateNewNode(double[] goal) =>
    new(goal.Subtract(_values).Normalize().Multiply(step).Add(_values), _cost + step, this);

  public void CompareParent(RRTStarNode node)
  {
    if (node._cost < _parent._cost)
    {
      _parent = node;
      _cost = node.Parent._cost + step;
    }
  }
}
