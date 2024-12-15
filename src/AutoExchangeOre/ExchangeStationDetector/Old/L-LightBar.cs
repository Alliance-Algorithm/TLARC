using System.Drawing;

namespace AutoExchange.ExchangeStationDetector.Old;

enum LLightBarType
{
    ForwardLong,
    ForwardShort,
    Beside
}
struct LLightBar
{
    public readonly (Point Point2D, Emgu.CV.Structure.MCvPoint3D32f Point3D) this[int index] => (point2D[index], point3D[index]);
    public Point center;
    public Point[] point2D;
    public Emgu.CV.Structure.MCvPoint3D32f[] point3D;
    public Vector2d forward;
    public LLightBarType type;
}


class LLightBarFilter
{
    const double sigmoidBias = 1;
    private double[] lastUpdateTime = [double.MaxValue, double.MaxValue, double.MaxValue, double.MaxValue, double.MaxValue, double.MaxValue];
    private LLightBar[] lLightBars = [
        new(){center = new(),point3D= [
            new(0,-126.5f,126.5f),
            new(0,-126.5f,108.5f),
            new(0,-137.5f,108.5f),
            new(0,-137.5f,137.5f),
            new(0,-108.5f,137.5f),
            new(0,-108.5f,126.5f),
        ]},
        new(){center = new(),point3D= [
            new(0,126.5f,126.5f),
            new(0,87.5f,126.5f),
            new(0,87.5f,137.5f),
            new(0,137.5f,137.5f),
            new(0,137.5f,87.5f),
            new(0,126.5f,87.5f),
        ]},
        new(){center = new(),point3D= [
            new(0,126.5f,-126.5f),
            new(0,126.5f,-87.5f),
            new(0,137.5f,-87.5f),
            new(0,137.5f,-137.5f),
            new(0,87.5f,-137.5f),
            new(0,87.5f,-126.5f),
        ]},
        new(){center = new(),point3D= [
            new(0,-126.5f,-126.5f),
            new(0,-87.5f,-126.5f),
            new(0,-87.5f,-137.5f),
            new(0,-137.5f,-137.5f),
            new(0,-137.5f,-87.5f),
            new(0,-126.5f,-87.5f),
        ]},
        new(){center = new(),point3D= [
            new(59.642136f,144f,0),
            new(145.5f,144f,85.857864f),
            new(145.5f,144f,100f),
            new(45.5f, 144,0f),
            new(145.5f,144f,-100f),
            new(145.5f,144f,-85.857864f),
        ]},
        new(){center = new(),point3D= [
            new(59.642136f,-144f,0),
            new(145.5f,-144f,-85.857864f),
            new(145.5f,-144f,-100f),
            new(45.5f, -144,0f),
            new(145.5f,-144f,100f),
            new(145.5f,-144f,85.857864f),
        ]},

    ];


    public List<LLightBar> FilterLightBar(List<LLightBar> lightBars)
    {
        List<LLightBar> ret = [];

        var forwardShort = lightBars.Where(x => x.type == LLightBarType.ForwardShort).ToList() ?? [];
        var forwardLong = lightBars.Where(x => x.type == LLightBarType.ForwardLong).ToList() ?? [];
        var besideShort = lightBars.Where(x => x.type == LLightBarType.Beside).ToList() ?? [];


        bool updateID1 = false;
        bool updateID2 = false;
        bool updateID3 = false;
        bool updateID4 = false;

        foreach (var lightBar in forwardShort)
        {
            if (CheckDistance(lightBar.center, lLightBars[0].center, lastUpdateTime[0]))
            {
                updateID1 = true;
                var tmp = lightBar;
                tmp.point3D = lLightBars[0].point3D;
                lLightBars[0].center = tmp.center;
                lastUpdateTime[0] = 0;
                ret.Add(tmp);
                break;
            }
        }

        int[] lightBarLongResult = [-1, -1, -1];
        if (forwardLong.Count != 0)
        {
            if (updateID1)
            {
                switch (forwardLong.Count)
                {
                    case 3:
                        {
                            updateID2 = true;
                            updateID3 = true;
                            updateID4 = true;
                            double min = double.MaxValue;
                            int index = -1;
                            for (int i = 0; i < 3; i++)
                            {
                                var tmp = forwardLong[i].forward.Normalized.Dot(forwardLong[(i + 1) % 3].forward.Normalized);
                                min = Math.Min(min, tmp);

                                if (min == tmp)
                                    index = (i + 2) % 3;
                            }

                            lightBarLongResult[1] = index;

                            if (forwardLong[index].forward.Cross(forwardLong[(index + 1) % 3].forward) < 0)
                            {
                                lightBarLongResult[2] = (index + 1) % 3;
                                lightBarLongResult[0] = (index + 2) % 3;
                            }
                            else
                            {
                                lightBarLongResult[0] = (index + 1) % 3;
                                lightBarLongResult[2] = (index + 2) % 3;
                            }
                            break;
                        }
                    case 2:
                        {
                            double min = forwardLong[0].forward.Normalized.Dot(forwardLong[1].forward.Normalized);
                            int index = -1;
                            for (int i = 0; i < 2; i++)
                            {
                                var tmp = ret[0].forward.Normalized.Dot(forwardLong[i].forward.Normalized);
                                min = Math.Min(min, tmp);

                                if (min == tmp)
                                    index = i;
                            }
                            if (index != -1)
                            {
                                lightBarLongResult[1] = index;
                                updateID3 = true;
                                if (ret[0].forward.Cross(forwardLong[(index + 1) % 2].forward) < 0)
                                {
                                    lightBarLongResult[0] = (index + 1) % 2;
                                    updateID2 = true;
                                }
                                else
                                {
                                    lightBarLongResult[2] = (index + 1) % 2;
                                    updateID4 = true;
                                }
                            }
                            else
                            {
                                updateID4 = true;
                                updateID2 = true;

                                if (ret[0].forward.Cross(forwardLong[0].forward) < 0)
                                {
                                    lightBarLongResult[0] = 0;
                                    lightBarLongResult[2] = 1;
                                }
                                else
                                {
                                    lightBarLongResult[2] = 0;
                                    lightBarLongResult[0] = 1;
                                }
                            }

                            break;
                        }
                    case 1:
                        {
                            if (ret[0].forward.Normalized.Dot(forwardLong[0].forward.Normalized) < -0.8)
                            {
                                lightBarLongResult[1] = 0;
                                updateID3 = true;
                            }

                            else if (ret[0].forward.Cross(forwardLong[0].forward) < 0)
                            {
                                lightBarLongResult[0] = 0;
                                updateID2 = true;
                            }
                            else
                            {
                                lightBarLongResult[2] = 0;
                                updateID4 = true;
                            }
                            break;
                        }
                    default:
                        break;
                }

            }
            else
            {
                PriorityQueue<(int index, double distance), double>[] priorityQueues = new PriorityQueue<(int index, double distance), double>[forwardLong.Count];

                for (int i = 0; i < forwardLong.Count; i++)
                {
                    priorityQueues[i] = new();
                    for (int j = 0; j < 3; j++)
                    {
                        var distance = Distance(forwardLong[i].center, lLightBars[j + 1].center);
                        priorityQueues[i].Enqueue((j, distance), distance);
                    }
                }
                (int index, double distance)[] tmpResult = [(-1, double.MaxValue), (-1, double.MaxValue), (-1, double.MaxValue)];
                bool[] bools = new bool[priorityQueues.Length];

                int index = 0;
                while (index < priorityQueues.Length)
                {
                    if (bools[index] == true)
                    {
                        index++;
                        continue;
                    }
                    (var indexForTarget, var distance) = priorityQueues[index].Dequeue();
                    bools[index] = true;
                    if (tmpResult[indexForTarget].distance > distance)
                    {
                        if (index > 3)
                        {
                            tmpResult[indexForTarget].index = index;
                        }
                        else if (tmpResult[indexForTarget].index != -1)
                        {
                            // bools[tmpResult[indexForTarget].index] = false;
                            continue;
                        }
                        else
                        {
                            tmpResult[indexForTarget] = (index, distance);
                            index++;
                        }
                    }
                }
                if (tmpResult[0].index != -1)
                {
                    lightBarLongResult[0] = tmpResult[0].index;
                    updateID2 = true;
                }
                if (tmpResult[1].index != -1)
                {
                    lightBarLongResult[1] = tmpResult[1].index;
                    updateID3 = true;
                }
                if (tmpResult[2].index != -1)
                {
                    lightBarLongResult[2] = tmpResult[2].index;
                    updateID4 = true;
                }
            }
        }
        if (besideShort.Count != 0 && !updateID1 && !updateID2 && !updateID3 && !updateID4)
        {
            if (besideShort[0].forward.x < 0)
            {
                var tmp = besideShort[0];
                tmp.point3D = lLightBars[5].point3D;
                lLightBars[5].center = tmp.center;
                ret.Add(tmp);
                lastUpdateTime[5] = 0;
            }
            else
            {
                var tmp = besideShort[0];
                tmp.point3D = lLightBars[4].point3D;
                lLightBars[4].center = tmp.center;
                ret.Add(tmp);
                lastUpdateTime[4] = 0;
            }
            if (updateID2 && updateID3)
            {
            }
            else if (updateID4 && updateID1)
            {

            }
        }

        if (updateID2)
        {
            var tmp = forwardLong[lightBarLongResult[0]];
            tmp.point3D = lLightBars[1].point3D;
            lLightBars[1].center = tmp.center;
            ret.Add(tmp);
            lastUpdateTime[1] = 0;
        }
        if (updateID3)
        {
            var tmp = forwardLong[lightBarLongResult[1]];
            tmp.point3D = lLightBars[2].point3D;
            lLightBars[2].center = tmp.center;
            ret.Add(tmp);
            lastUpdateTime[2] = 0;
        }
        if (updateID4)
        {
            var tmp = forwardLong[lightBarLongResult[2]];
            tmp.point3D = lLightBars[3].point3D;
            lLightBars[3].center = tmp.center;
            ret.Add(tmp);
            lastUpdateTime[3] = 0;
        }
        return ret;
    }

    private static bool CheckDistance(Point p1, Point p2, double time)
    {
        return Distance(p1, p2) < Epsilon(time);

    }

    static double Epsilon(double time)
    {
        return 1e7 / (1 + Math.Exp(-time + sigmoidBias));
    }

    static double Distance(Point pt1, Point pt2)
    {
        return Math.Sqrt(Math.Pow(pt1.X - pt2.X, 2) + Math.Pow(pt1.Y - pt2.Y, 2));
    }

}
