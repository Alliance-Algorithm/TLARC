using System.Drawing;
using Emgu.CV.Structure;

namespace AutoExchange.ExchangeStationDetector;

static class MaxDistanceSampler
{

    public static List<(Point Point2D, MCvPoint3D32f Point3D)> Sample(List<LLightBar> points, int sampleSize)
    {
        List<(Point Point2D, MCvPoint3D32f Point3D)> sampledPoints = [];

        // 随机选择第一个点
        Random rnd = new Random();
        var tmp = points[rnd.Next(points.Count)];
        var tmpIndex = rnd.Next(6);
        (Point, MCvPoint3D32f) firstPoint = tmp[tmpIndex];
        sampledPoints.Add(firstPoint);

        for (int i = 1; i < sampleSize; i++)
        {
            (Point Point2D, MCvPoint3D32f Point3D) farthestPoint = GetFarthestPoint(points, sampledPoints);
            sampledPoints.Add(farthestPoint);
        }

        return sampledPoints;
    }

    static (Point Point2D, MCvPoint3D32f Point3D) GetFarthestPoint(List<LLightBar> points, List<(Point Point2D, MCvPoint3D32f Point3D)> sampledPoints)
    {

        (Point Point2D, MCvPoint3D32f Point3D) farthestPoint = new();
        double maxDistance = double.MinValue;

        foreach (var point in points)
        {
            for (int i = 0; i < 6; i++)
            {
                double minDistance = 0;
                bool isInSample = false;

                foreach (var (Point2D, Point3D) in sampledPoints)
                {
                    double distance = Distance(point.point3D[i], Point3D);
                    minDistance += distance;
                    if (point[i].Point2D == Point2D)
                    {
                        isInSample = true;
                        break;
                    }
                }

                if (isInSample)
                    continue;

                if (minDistance > maxDistance)
                {
                    maxDistance = minDistance;
                    farthestPoint = point[i];
                }
            }
        }

        return farthestPoint;
    }

    static double Distance(MCvPoint3D32f pt1, MCvPoint3D32f pt2)
    {
        return Math.Sqrt(Math.Pow(pt1.X - pt2.X, 2) + Math.Pow(pt1.Y - pt2.Y, 2) + Math.Pow(pt1.Z - pt2.Z, 2));
    }
}
