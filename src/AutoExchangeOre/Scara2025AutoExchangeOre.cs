using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using g4;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using Utils;

class Scara2025AutoExchangeOre
{
    readonly IForwardDynamic forwardDynamic;
    readonly IInverseDynamicSolver inverseDynamicSolver;
    readonly Scara2025BepuDetector colliderDetector;
    readonly PathToPathLoose searcher;

    const double step = 2;
    double[] _beginThetas = new double[6] { -1.7589654251491529, 0.7, -2.0042416738476305, -1.570796248551242, 1.570796217569299, 0 };

    public Scara2025AutoExchangeOre()
    {
        forwardDynamic = new Scara2025ForwardDynamic();
        inverseDynamicSolver = new Scara2025InverseDynamic(0.3, 0.3, 0.05, (-Math.PI, Math.PI), (-Math.PI, Math.PI), (-120 * Math.PI / 180, 120 * Math.PI / 180));
        colliderDetector = new Scara2025BepuDetector();
        searcher = new(
            new RRT_BHAStar(
                new double[] { Math.PI, 0.8, Math.PI, Math.PI, Math.PI, Math.PI },
                new double[] { -Math.PI, 0, -Math.PI, -Math.PI, -Math.PI, -Math.PI })
            {
                forwardDynamic = forwardDynamic,
                obstacleDetector = colliderDetector
            }
        );

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

    public List<BSplineTrajectoryWithMinimalSnap> PlanTrajectory((Vector3d position, Quaterniond rotation) target)
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
        // tmp[0][0].AddLast((_beginThetas[0], 0));
        // tmp[0][1].AddLast((_beginThetas[1], 0));
        // tmp[0][2].AddLast((_beginThetas[2], 0));
        // tmp[0][3].AddLast((_beginThetas[3], 0));
        // tmp[0][4].AddLast((_beginThetas[4], 0));
        // tmp[0][5].AddLast((_beginThetas[5], 0));

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
        var path = searcher.Search(_beginThetas, thetaMid);

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

        return tmpSplineTo;
    }
}