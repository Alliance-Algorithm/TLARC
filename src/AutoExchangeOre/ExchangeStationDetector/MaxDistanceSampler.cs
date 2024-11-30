using System.Drawing;
using Emgu.CV.Structure;

namespace AutoExchange.ExchangeStationDetector;

static class MaxDistanceSampler
{

    public static List<(PointF Point2D, MCvPoint3D32f Point3D)> MaxDistanceSampling(List<LLightBar> points, int sampleSize)
    {
        List<(PointF Point2D, MCvPoint3D32f Point3D)> sampledPoints = [];

        // 随机选择第一个点
        Random rnd = new Random();
        var tmp = points[rnd.Next(points.Count)];
        var tmpIndex = rnd.Next(6);
        (PointF, MCvPoint3D32f) firstPoint = tmp[tmpIndex];
        sampledPoints.Add(firstPoint);

        for (int i = 1; i < sampleSize; i++)
        {
            (PointF Point2D, MCvPoint3D32f Point3D) farthestPoint = GetFarthestPoint(points, sampledPoints);
            sampledPoints.Add(farthestPoint);
        }

        return sampledPoints;
    }

    static (PointF Point2D, MCvPoint3D32f Point3D) GetFarthestPoint(List<LLightBar> points, List<(PointF Point2D, MCvPoint3D32f Point3D)> sampledPoints)
    {

        (PointF Point2D, MCvPoint3D32f Point3D) farthestPoint = new();
        double maxDistance = double.MinValue;

        foreach (var point in points)
        {
            for (int i = 0; i < 6; i++)
            {
                double minDistance = double.MaxValue;

                foreach (var (Point2D, Point3D) in sampledPoints)
                {
                    double distance = Distance(point.point2D[i], Point2D);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                    }
                }

                if (minDistance > maxDistance)
                {
                    maxDistance = minDistance;
                    farthestPoint = point[i];
                }
            }
        }

        return farthestPoint;
    }

    static double Distance(PointF pt1, PointF pt2)
    {
        return Math.Sqrt(Math.Pow(pt1.X - pt2.X, 2) + Math.Pow(pt1.Y - pt2.Y, 2));
    }
}
