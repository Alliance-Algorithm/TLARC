
using RapidlyArmPlanner.ArmSolver.ForwardDynamic;
using RapidlyArmPlanner.ColliderDetector;

namespace RapidlyArmPlanner.PathFinder.RRT_BHAStar;

class RRT_BHAStarNode
{
    double[] _values;

    public double[] Values => _values;

    double _cost = 0;

    double _heuristic = 0;

    public double Heuristic => _heuristic;
    public double Cost => _cost;

    RRT_BHAStarNode? _parent = null;
    public RRT_BHAStarNode? Parent => _parent;
    const double step = 0.08;
    const int childrenCount = 30;


    public RRT_BHAStarNode(double[] values, double cost = 0, RRT_BHAStarNode? parent = null)
    {
        _values = values;
        _cost = cost;

        _parent = parent ?? this;
    }

    public List<RRT_BHAStarNode> GenerateNewNode(IForwardDynamic forwardDynamic, IColliderDetector obstacleDetector)
    {
        Random rand = new();
        List<RRT_BHAStarNode> news = new();
        double[] direction = new double[_values.Length];
        for (int i = 0; i < childrenCount; i++)
        {
            RRT_BHAStarNode? node = null;
            while (true)
            {
                for (int j = 0; j < _values.Length; j++)
                    direction[j] = rand.NextDouble() * 2 - 1;

                direction = direction.Normalize();
                node = new(direction.Multiply(step).Add(_values), _cost + step, this);
                List<(Vector3d pos, Quaterniond rotation)> pose = forwardDynamic.GetPose(node.Values);
                // if (!obstacleDetector.Detect(pose))
                    break;
            }
            news.Add(node);
        }
        return news;
    }
    public double CalculateDistance(double[] node) => _values.Subtract(node).SquareEuclidean();

    public void CalculateTotalCost(RRT_BHAStarNode target)
    {
        _heuristic = target._values.Subtract(_values).SquareEuclidean();
    }
    public void CompareParent(RRT_BHAStarNode node)
    {
        if (node._cost < _parent._cost)
        {
            _parent = node;
            _cost = node.Parent._cost + step;
        }
    }
}