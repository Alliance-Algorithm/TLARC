// RNG-BiAStar（Random Node Generation Bi-A)
using RapidlyArmPlanner.ArmSolver.ForwardDynamic;
using RapidlyArmPlanner.ColliderDetector;

namespace RapidlyArmPlanner.PathFinder.RRT_BHAStar;
class RRT_BHAStar : IPathFinder<double>
{

    public IForwardDynamic forwardDynamic { get; set; }
    public IColliderDetector obstacleDetector { get; set; }
    double[] _upperBound;
    double[] _lowerBound;
    public RRT_BHAStar(
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
    public List<LinkedList<double>> Search(double[] start, double[] target)
    {
        List<RRT_BHAStarNode> totalNodesStart = new();
        List<RRT_BHAStarNode> totalNodesEnd = new();
        PriorityQueue<RRT_BHAStarNode, double> openListStart = new();
        PriorityQueue<RRT_BHAStarNode, double> openListEnd = new();
        List<LinkedList<double>> path = new();

        RRT_BHAStarNode startNode = new(start);
        RRT_BHAStarNode endNode = new(target);

        openListStart.Enqueue(startNode, 0);
        openListEnd.Enqueue(endNode, 0);
        RRT_BHAStarNode? tailStartNode = null;
        RRT_BHAStarNode? tailEndNode = null;

        bool findAlready = false;
        while (openListStart.Count > 0 && openListEnd.Count > 0)
        {

            var currentStart = openListStart.Dequeue();
            var currentEnd = openListEnd.Dequeue();

            var nodes = currentStart.GenerateNewNode(forwardDynamic, obstacleDetector);
            foreach (var node in nodes)
            {
                // List<(Vector3d pos, Quaterniond rotation)> pose = forwardDynamic.GetPose(node.Values);
                // if (obstacleDetector.Detect(pose))
                //     continue;
                bool isIn = true;
                for (int j = 0; j < node.Values.Length; j++)
                    if (node.Values[j] < _lowerBound[j] || node.Values[j] > _upperBound[j])
                    {
                            
                        break;
                    }
                if (!isIn)
                    continue;

                LinkedList<int> nearNodes = new();
                for (int j = 0, k = totalNodesStart.Count; j < k; j++)
                    if (totalNodesStart[j].CalculateDistance(node.Values) < 0.2)
                        nearNodes.AddLast(j);

                var index = nearNodes.First;
                while (index != null)
                {
                    node.CompareParent(totalNodesStart[index.Value]);
                    index = index.Next;
                }
                node.CalculateTotalCost(endNode);
                for (int j = 0; j < totalNodesEnd.Count; j++)
                {
                    var c = totalNodesEnd[j].Values.Subtract(node.Values).SquareEuclidean();
                    if (c < 0.1)
                    {
                        tailStartNode = currentStart;
                        tailEndNode = totalNodesEnd[j];
                        findAlready = true;
                    }
                    if (findAlready) break;
                }
                if (findAlready) break;
                totalNodesStart.Add(node);
                openListStart.Enqueue(node, node.Cost + node.Heuristic);
            }
            if (findAlready) break;
            nodes = currentEnd.GenerateNewNode(forwardDynamic, obstacleDetector);
            foreach (var node in nodes)
            {
                // List<(Vector3d pos, Quaterniond rotation)> pose = forwardDynamic.GetPose(node.Values);
                // if (obstacleDetector.Detect(pose))
                //     continue;

                bool isIn = true;
                for (int j = 0; j < node.Values.Length; j++)
                    if (node.Values[j] < _lowerBound[j] || node.Values[j] > _upperBound[j])
                    {
                        isIn = false;
                        break;
                    }
                if (!isIn)
                    continue;
                LinkedList<int> nearNodes = new();
                for (int j = 0, k = totalNodesEnd.Count; j < k; j++)
                    if (totalNodesEnd[j].CalculateDistance(node.Values) < 0.2)
                        nearNodes.AddLast(j);

                var index = nearNodes.First;
                while (index != null)
                {
                    node.CompareParent(totalNodesEnd[index.Value]);
                    index = index.Next;
                }
                node.CalculateTotalCost(startNode);
                for (int j = 0; j < totalNodesStart.Count; j++)
                {
                    var c = totalNodesStart[j].Values.Subtract(node.Values).SquareEuclidean();
                    if (c < 0.1)
                    {
                        tailStartNode = totalNodesStart[j];
                        tailEndNode = currentEnd;
                        findAlready = true;
                    }
                    if (findAlready) break;
                }
                if (findAlready) break;
                totalNodesEnd.Add(node);
                openListEnd.Enqueue(node, node.Cost + node.Heuristic);
            }
            if (findAlready) break;
        }

        for (int i = 0; i < start.Length; i++) path.Add(new());

        if (!findAlready) return path;
        while (true)
        {
            for (int i = 0; i < tailStartNode.Values.Length; i++) path[i].AddFirst(tailStartNode.Values[i]);
            if (tailStartNode.Parent == tailStartNode)
                break;
            tailStartNode = tailStartNode.Parent;
        }
        while (true)
        {
            for (int i = 0; i < tailEndNode.Values.Length; i++) path[i].AddLast(tailEndNode.Values[i]);
            if (tailEndNode.Parent == tailEndNode)
                break;
            tailEndNode = tailEndNode.Parent;
        }
        return path;
    }
}