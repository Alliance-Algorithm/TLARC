using System.Drawing;

namespace AutoExchange.ExchangeStationDetector;

static class MaxDistanceSampler
{

    public static List<PointF> MaxDistanceSampling(List<PointF> points, int sampleSize)
    {
        List<PointF> sampledPoints = [];

        // 随机选择第一个点
        Random rnd = new Random();
        PointF firstPoint = points[rnd.Next(points.Count)];
        sampledPoints.Add(firstPoint);

        for (int i = 1; i < sampleSize; i++)
        {
            PointF farthestPoint = GetFarthestPoint(points, sampledPoints);
            sampledPoints.Add(farthestPoint);
        }

        return sampledPoints;
    }

    static PointF GetFarthestPoint(List<PointF> points, List<PointF> sampledPoints)
    {
        PointF farthestPoint = new();
        double maxDistance = double.MinValue;

        foreach (var point in points)
        {
            double minDistance = double.MaxValue;

            foreach (var sampledPoint in sampledPoints)
            {
                double distance = Distance(point, sampledPoint);
                if (distance < minDistance)
                {
                    minDistance = distance;
                }
            }

            if (minDistance > maxDistance)
            {
                maxDistance = minDistance;
                farthestPoint = point;
            }
        }

        return farthestPoint;
    }

    static double Distance(PointF pt1, PointF pt2)
    {
        return Math.Sqrt(Math.Pow(pt1.X - pt2.X, 2) + Math.Pow(pt1.Y - pt2.Y, 2));
    }
}
