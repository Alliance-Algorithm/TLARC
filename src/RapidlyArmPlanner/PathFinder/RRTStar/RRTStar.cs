
using RapidlyArmPlanner.ArmSolver.ForwardDynamic;
using RapidlyArmPlanner.ColliderDetector;


namespace RapidlyArmPlanner.PathFinder.RRTStar;
class RRTStar
{
    public IForwardDynamic forwardDynamic { get; set; }
    public IColliderDetector obstacleDetector { get; set; }

    int iterator = 10000;
    public double goalBias = 0.1;
    double[] _upperBound;
    double[] _lowerBound;
    readonly double searchLength = 0.5;
    public RRTStar(
    double[] upperBound,
    double[] lowerBound)
    {
        _upperBound = upperBound;
        _lowerBound = lowerBound;
        for (int i = 0; i < _upperBound.Length; i++)
        {
            _lowerBound[i] = Math.Max(_lowerBound[i], -12);
            _upperBound[i] = Math.Min(12, _upperBound[i]);
        }
        forwardDynamic = new Scara2025ForwardDynamic();
        obstacleDetector = new Scara2025BepuDetector();
    }

    public List<LinkedList<double>>? Search(double[] start, double[] target)
    {
        RRTStarNode? tail = null;
        Random random = new();
        List<RRTStarNode> treeA = [new RRTStarNode(start)];
        int len = start.Length;
        double[] goal = new double[len];
        for (int i = 0; true; i++)
        {
            if (random.NextDouble() < goalBias)
                goal = target.Copy();
            else
                for (int j = 0; j < len; j++)
                    goal[j] = random.NextDouble() * (_upperBound[j] - _lowerBound[j]) + _lowerBound[j];

            double minValue = double.MaxValue;
            int minElement = -1;
            for (int j = 0, k = treeA.Count; j < k; j++)
            {
                var number = treeA[j].CalculateDistance(goal);
                if (number < minValue)
                {
                    minValue = number;
                    minElement = j;
                }
            }

            var nodeNew = treeA[minElement].GenerateNewNode(goal);

            List<(Vector3d pos, Quaterniond rotation)> pose = forwardDynamic.GetPose(nodeNew.Values);
            if (obstacleDetector.Detect(pose))
                continue;

            LinkedList<int> nearNodes = new();
            for (int j = 0, k = treeA.Count; j < k; j++)
                if (treeA[j].CalculateDistance(nodeNew.Values) < searchLength)
                    nearNodes.AddLast(j);

            var index = nearNodes.First;
            while (index != null)
            {
                nodeNew.CompareParent(treeA[index.Value]);
                index = index.Next;
            }
            index = nearNodes.First;
            // while (index != null)
            // {
            //     treeA[index.Value].CompareParent(nodeNew);
            //     index = index.Next;
            // }

            treeA.Add(nodeNew);

            if (Math.Abs(nodeNew.Values.Subtract(target).Euclidean()) < 0.2)
            {
                tail = nodeNew;
                break;
            }
        }
        if (tail == null)
            return null;
        List<LinkedList<double>> path = new();

        for (int i = 0; i < tail.Values.Length; i++) path.Add(new());

        while (true)
        {
            for (int i = 0; i < tail.Values.Length; i++) path[i].AddFirst(tail.Values[i]);
            tail = tail.Parent;
            if (tail.Parent == tail)
                break;
        }
        for (int i = 0; i < tail.Values.Length; i++) path[i].AddFirst(start[i]);
        for (int i = 0; i < tail.Values.Length; i++) path[i].AddLast(target[i]);

        return path;
    }
}