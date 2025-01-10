
using RapidlyArmPlanner.ArmSolver.ForwardDynamic;
using RapidlyArmPlanner.ArmSolver.InverseDynamic;
using RapidlyArmPlanner.ColliderDetector;
using RapidlyArmPlanner.PathFinder.RRT_BHAStar;
using RapidlyArmPlanner.TrajectoryFit;
namespace AutoExchange;
class ArmPlanner
{

    required public IForwardDynamic forwardDynamic { get; init; }
    required public IInverseDynamicSolver inverseDynamicSolver { get; init; }
    required public IColliderDetector colliderDetector { get; init; }
    required public PathToPathLoose searcher { get; init; }

    const double step = 2;


    public ArmPlanner()
    {
        // forwardDynamic = new Scara2025ForwardDynamic();
        // inverseDynamicSolver = new Scara2025InverseDynamic(0.3, 0.3, 0.05, (-Math.PI, Math.PI), (-Math.PI, Math.PI), (-120 * Math.PI / 180, 120 * Math.PI / 180));
        // colliderDetector = new Scara2025BepuDetector();
        // searcher = new(
        //     new RRT_BHAStar(
        //         [Math.PI, 0.8, Math.PI, Math.PI, Math.PI, Math.PI],
        //         [-Math.PI, 0, -Math.PI, -Math.PI, -Math.PI, -Math.PI])
        //     {
        //         forwardDynamic = forwardDynamic,
        //         obstacleDetector = colliderDetector
        //     }
        // );

    }

    List<List<LinkedList<(double value, double loose)>>> Combine(List<List<LinkedList<(double value, double loose)>>> a, List<double[]> b, List<List<double[]>> doubles)
    {
        List<List<LinkedList<(double value, double loose)>>> tmp = new();
        for (int i = 0; i < a.Count; i++)
            for (int j = 0; j < b.Count; j++)
            {
                var tmp1 = new List<LinkedList<(double value, double loose)>>();
                for (int k = 0; k < b[j].Length; k++)
                {
                    tmp1.Add(new LinkedList<(double, double)>(a[i][k]));
                    tmp1[^1].AddLast((b[j][k], 0));
                }
                tmp.Add(tmp1);
            }

        if (doubles == null || doubles.Count == 0)
            return tmp;

        var tmp2 = doubles[0];
        doubles.RemoveAt(0);
        return Combine(tmp, tmp2, doubles);
    }

    public bool PlanTrajectory(double[] beginThetas, (Vector3d position, Quaterniond rotation) target, out List<BSplineTrajectoryWithMinimalSnap> trajectory)
    {
        var rotation2 = target.rotation;
        var position2 = target.position;
        var rotation1 = target.rotation;
        var position1 = target.position - target.rotation * Vector3d.AxisX * 0.3f;

        List<List<LinkedList<(double value, double loose)>>> tmp = new() { new() {
            new() {  },
                new() {  },
                new () {},
                new() { },
                new() { },
                new() { },
         }         };

        inverseDynamicSolver.Solve((position1, rotation1), out var inners);
        inverseDynamicSolver.Solve(target, out var thetas);
        Vector3d Position = -(target.rotation * Vector3d.AxisX * 0.3f);
        List<List<double[]>> innerThetas = new();
        for (int i = 0; i <= step; i++)
        {
            Vector3d pos = Position * (step - i) / step + target.position;
            ref Quaterniond rotation = ref rotation2;
            if (inverseDynamicSolver.Solve((pos, rotation), out var innerTheta))
                innerThetas.Add(innerTheta);
        }
        List<List<LinkedList<(double value, double loose)>>> result = Combine(tmp, inners, innerThetas);

        PriorityQueue<int, double> trajectories = new();
        for (int i = 0; i < result.Count; i++)
        {
            var theta = result[i];
            var tmpSpline = new BSplineTrajectoryWithMinimalSnap[theta.Count];
            for (int j = 0; j < theta.Count; j++)
                tmpSpline[j] = new BSplineTrajectoryWithMinimalSnap(theta[j]);
            BSplineTrajectoryWithMinimalSnap.ReAllocTimeline(tmpSpline.ToList());

            trajectories.Enqueue(i, tmpSpline.Max(x => x.Value));
        }
        var index = trajectories.Dequeue();

        List<LinkedList<(double value, double loose)>> tail = new();
        for (int i = 0; i < result[index].Count; i++)
        {
            LinkedList<(double, double)> doubles = new();
            var node = result[index][i].Last;
            for (int j = 0; j <= step; j++)
            {
                doubles.AddFirst(node.Value);
                node = node.Previous;
            }
            tail.Add(doubles);
        }

        double[] thetaMid = tail.Select(x => x.First.Value.value).ToArray();

        colliderDetector.Update(forwardDynamic.GetPose(thetas[0]), target);
        var path = searcher.Search(beginThetas, thetaMid);

        for (int i = 0; i < path.Count; i++)
        {
            path[i].RemoveLast();
            path[i].AddLast(tail[i].First.Value);
            foreach (var item in tail[i])
                path[i].AddLast(item);
        }

        var tmpSplineTo = new List<BSplineTrajectoryWithMinimalSnap>();
        for (int i = 0; i < path.Count; i++)
            tmpSplineTo.Add(new BSplineTrajectoryWithMinimalSnap(path[i]));
        BSplineTrajectoryWithMinimalSnap.ReAllocTimeline(tmpSplineTo);

        trajectory = tmpSplineTo;
        return path.All(x => x.Count > 0);
    }
}