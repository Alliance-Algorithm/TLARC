
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Util;
using TlarcKernel.IO.ProcessCommunicateInterfaces;

namespace AutoExchange.ExchangeStationDetector;

class ExchangeStationDetector : Component
{

    public ReadOnlyUnmanagedSubscription<Mat> rawImage = new("/image/raw");
    public ReadOnlyUnmanagedInterfacePublisher<Mat> blurredPub = new("/image/blurred");
    public ReadOnlyUnmanagedInterfacePublisher<Mat> edgesPub = new("/image/edges");
    public ReadOnlyUnmanagedInterfacePublisher<Mat> approxPub = new("/image/approx");
    public bool IsBlue = false;
    public double epsilonCoefficient = 0.01f;
    public double minArea = 500;
    public Vector2d thresholdMinMax = new() { x = 70, y = 255 };
    public override void Start()
    {
        thresholdMinMax.y = Math.Clamp(thresholdMinMax.y, 0, 255);
        thresholdMinMax.x = Math.Clamp(thresholdMinMax.x, 0, thresholdMinMax.y);
    }
    public static Mat SharpenImage(Mat image)
    {
        Mat sharpened = new Mat(image.Height, image.Width, image.Depth, image.NumberOfChannels);
        Mat kernel = new Mat(new System.Drawing.Size(3, 3), Emgu.CV.CvEnum.DepthType.Cv64F, 1);
        kernel.SetTo(new double[] { -1, -1, -1, -1, 9, -1, -1, -1, -1 });
        CvInvoke.Filter2D(image, sharpened, kernel, new System.Drawing.Point(-1, -1));
        return sharpened;
    }


    public override void Update()
    {
        using var raw = rawImage.Rent;
        if (raw == null) return;
        Mat image = raw.Instance.Value.Clone();
        Mat blurred = new();
        Mat edges = new();
        Mat[] channels = image.Split();
        using Mat blueChannel = channels[0] - channels[2];
        using Mat redChannel = channels[2] - channels[0];
        channels[0].Dispose();
        channels[1].Dispose();
        channels[2].Dispose();
        if (IsBlue)
        {
            using var blue = SharpenImage(blueChannel);
            CvInvoke.GaussianBlur(blue, blurred, new System.Drawing.Size(5, 5), 0);
        }
        else
        {
            using var red = SharpenImage(redChannel);
            CvInvoke.GaussianBlur(red, blurred, new System.Drawing.Size(5, 5), 0);
        }

        CvInvoke.Threshold(blurred, blurred, thresholdMinMax.x, thresholdMinMax.y, Emgu.CV.CvEnum.ThresholdType.Binary);
        CvInvoke.Canny(blurred, edges, 50, 150);
        using Mat kernel = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Rectangle, new System.Drawing.Size(3, 3), new System.Drawing.Point(-1, -1));
        CvInvoke.Dilate(edges, edges, kernel, new System.Drawing.Point(-1, -1), 1, Emgu.CV.CvEnum.BorderType.Default, new Emgu.CV.Structure.MCvScalar(1));
        CvInvoke.Erode(edges, edges, kernel, new System.Drawing.Point(-1, -1), 1, Emgu.CV.CvEnum.BorderType.Default, new Emgu.CV.Structure.MCvScalar(1));

        using var contours = new VectorOfVectorOfPoint();
        CvInvoke.FindContours(edges, contours, null, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
        int width = image.Width; int height = image.Height;
        List<Point> vectorOfPoint = new();
        List<(Point point, int index, double length)> LPositions = new();
        for (int i = 0; i < contours.Size; i++)
        {
            using var contour = contours[i];
            var area = CvInvoke.ContourArea(contours[i]);
            if (area < minArea)
                continue;
            double length = CvInvoke.ArcLength(contour, true);
            double epsilon = epsilonCoefficient * CvInvoke.ArcLength(contour, true);
            using var approx = new VectorOfPoint();
            CvInvoke.ApproxPolyDP(contour, approx, epsilon, true);
            if (CvInvoke.IsContourConvex(approx))
            {
                using var moments = CvInvoke.Moments(approx, true);
                int centerX = (int)(moments.M10 / moments.M00); int centerY = (int)(moments.M01 / moments.M00);
                vectorOfPoint.Add(new(centerX, centerY));
                CvInvoke.DrawContours(image, contours, i, new Emgu.CV.Structure.MCvScalar(255, 0, 255), 2);
                continue;
            }
            if (approx.Size == 4)
                CvInvoke.ApproxPolyDP(contour, approx, epsilon / 2, true);

            if (approx.Size == 6 && !CvInvoke.IsContourConvex(approx))
            {
                using Subdiv2D subdiv2D = new Subdiv2D(new Rectangle(0, 0, width, height));
                for (int j = 0; j < approx.Size; j++)
                    subdiv2D.Insert(approx[j]);
                var triangles = subdiv2D.GetDelaunayTriangles();
                PriorityQueue<double, double> allAngles = new();
                PriorityQueue<double, double> allAnglesInverse = new();
                if (triangles.Length < 4)
                    continue;
                foreach (var triangle in triangles)
                {
                    Point pt1 = new((int)triangle.V0.X, (int)triangle.V0.Y);
                    Point pt2 = new((int)triangle.V1.X, (int)triangle.V1.Y);
                    Point pt3 = new((int)triangle.V2.X, (int)triangle.V2.Y);

                    double a = Distance(pt2, pt3);
                    double b = Distance(pt1, pt3);
                    double c = Distance(pt1, pt2);

                    double angleA = CalculateAngle(b, c, a);
                    double angleB = CalculateAngle(a, c, b);
                    double angleC = CalculateAngle(a, b, c);
                    var tmp = Math.Min(angleA, Math.Min(angleB, angleC));
                    allAngles.Enqueue(tmp, tmp);
                    allAnglesInverse.Enqueue(tmp, -tmp);
                    if (IsPointWithinImage(pt1, width, height) &&
                    IsPointWithinImage(pt2, width, height) &&
                    IsPointWithinImage(pt3, width, height))
                    {
                        CvInvoke.Line(image, pt1, pt2, new(255, 0, 0), 1);
                        CvInvoke.Line(image, pt2, pt3, new(255, 0, 0), 1);
                        CvInvoke.Line(image, pt3, pt1, new(255, 0, 0), 1);
                    }
                }
                bool NotTypeA = false;
                for (int j = 0; j < 3; j++)
                {
                    NotTypeA = allAngles.Dequeue() > 10;
                    if (NotTypeA)
                        break;
                }
                if (!NotTypeA)
                    CvInvoke.DrawContours(image, contours, i, new Emgu.CV.Structure.MCvScalar(255, 0, 0), 2);
                else
                {
                    using var moments = CvInvoke.Moments(approx, true);
                    for (int j = 0, count = approx.Size; j < count; j++)
                    {
                        Point p1 = approx[j];
                        Point p2 = approx[(j + 1) % count];
                        Point p3 = approx[(j + 2) % count];

                        Vector2d v1 = new(p1.X - p2.X, p1.Y - p2.Y);
                        Vector2d v2 = new(p2.X - p3.X, p2.Y - p3.Y);



                        if (v2.Cross(v1) < 0)
                        {
                            Vector2d v3 = new(p3.X + p1.X - p2.X - p2.X, p3.Y + p1.Y - p2.Y - p2.Y);
                            CvInvoke.Circle(image, p2, 1, new Emgu.CV.Structure.MCvScalar(255, 0, 0), 6);
                            CvInvoke.Line(image, p2, new((int)v3.x + p2.X, (int)v3.y + p2.Y), new Emgu.CV.Structure.MCvScalar(255, 0, 0), 3);
                            LPositions.Add((p2, i, length));
                        }
                    }
                    // LPositions.Add((new(centerX, centerY), i, length));
                }
            }
            for (int j = 0; j < approx.Size; j++)
                CvInvoke.Circle(image, approx[j], 1, new Emgu.CV.Structure.MCvScalar(0, 255, 0));
        }
        foreach (var (point, index, length) in LPositions)
        {
            if (FindNearestPoint(point, vectorOfPoint, length / 2))
                CvInvoke.DrawContours(image, contours, index, new Emgu.CV.Structure.MCvScalar(255, 255, 0), 2);
            else
                CvInvoke.DrawContours(image, contours, index, new Emgu.CV.Structure.MCvScalar(0, 255, 0), 2);
        }
        approxPub.LoadInstance(ref image);
        blurredPub.LoadInstance(ref blurred);
        edgesPub.LoadInstance(ref edges);
    }
    static bool IsPointWithinImage(Point pt, int width, int height) { return pt.X >= 0 && pt.X < width && pt.Y >= 0 && pt.Y < height; }
    static double Distance(PointF pt1, PointF pt2) { return Math.Sqrt(Math.Pow(pt1.X - pt2.X, 2) + Math.Pow(pt1.Y - pt2.Y, 2)); }
    static double CalculateAngle(double a, double b, double c)
    {
        double cosAngle = (a * a + b * b - c * c) / (2 * a * b);
        return Math.Acos(cosAngle) * (180.0 / Math.PI);
    }
    static bool FindNearestPoint(Point referencePoint, List<Point> points, double minDistance)
    {
        double minDistance_ = minDistance;
        foreach (var point in points)
        {
            double distance = Distance(referencePoint, point);
            if (distance < minDistance_)
            {
                minDistance_ = distance;
                return true;
            }
        }
        return false;
    }
}