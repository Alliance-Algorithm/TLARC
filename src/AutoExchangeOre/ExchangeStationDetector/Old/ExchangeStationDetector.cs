
using System.Drawing;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.Util;
using Intel.RealSense;
using Intel.RealSense.Math;
using TlarcKernel.IO.ProcessCommunicateInterfaces;

namespace AutoExchange.ExchangeStationDetector;

class ExchangeStationDetector : Component
{

    public ReadOnlyUnmanagedSubscription<Points> pointCloudSub = new("/real_sense/frame/pc");
    public ReadOnlyUnmanagedSubscription<Mat> depthSub = new("/real_sense/depth");
    public ReadOnlyUnmanagedSubscription<Mat> rawImage = new("/image/raw");
    public ReadOnlyUnmanagedInterfacePublisher<Mat> blurredPub = new("/image/blurred");
    public ReadOnlyUnmanagedInterfacePublisher<Mat> edgesPub = new("/image/edges");
    public ReadOnlyUnmanagedInterfacePublisher<Mat> approxPub = new("/image/approx");
    public bool IsBlue = false;
    public double epsilonCoefficient = 0.01f;
    public double minArea = 100;
    public Vector2d thresholdMinMax = new() { x = 128, y = 255 };

    LLightBarFilter lLightBarFilter = new();


    PnpSolver solver = new();

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
        using var pc = pointCloudSub.Rent;
        if (pc == null)
            return;
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
            CvInvoke.MedianBlur(redChannel, blurred, 5);
            using Mat kernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(5, 5), new Point(-1, -1));
            CvInvoke.MorphologyEx(blurred, blurred, MorphOp.Close, kernel, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(1));
        }

        CvInvoke.Canny(blurred, edges, 5, 6000, 5, false);

        using var contours = new VectorOfVectorOfPoint();
        CvInvoke.FindContours(edges, contours, null, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
        int width = image.Width; int height = image.Height;
        List<Point> vectorOfPoint = new();
        List<(LLightBar lLightBar, int index, double length)> LPositions = [];
        List<LLightBar> bar = [];
        for (int i = 0; i < contours.Size; i++)
        {
            using var contour = contours[i];
            var area = CvInvoke.ContourArea(contours[i]);
            if (area < minArea)
                continue;
            double length = CvInvoke.ArcLength(contour, true);
            double epsilon = epsilonCoefficient * length;
            using var approx = new VectorOfPoint();
            CvInvoke.ApproxPolyDP(contour, approx, epsilon, true);
            if (CvInvoke.IsContourConvex(approx))
            {
                using var moments = CvInvoke.Moments(approx, true);
                int centerX = (int)(moments.M10 / moments.M00); int centerY = (int)(moments.M01 / moments.M00);
                double max = double.MinValue;
                for (int k = 0; k < approx.Size; k++)
                    max = Math.Max(Distance(approx[k], new(centerX, centerY)), max);
                if (max > length / 4.5)
                    continue;
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
                    NotTypeA = allAngles.Dequeue() > 12;
                    if (NotTypeA)
                        break;
                }
                if (!NotTypeA)
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
                            List<Point> points1 = [];
                            for (int k = 0; k < 6; k++)
                                points1.Add(approx[(j + k + 1) % count]);
                            bar.Add(new() { center = new(p2.X, p2.Y), forward = v3, point2D = [.. points1], type = LLightBarType.Beside });
                            CvInvoke.Circle(image, p2, 1, new Emgu.CV.Structure.MCvScalar(0, 255, 0), 6);
                            CvInvoke.Line(image, p2, new((int)v3.x + p2.X, (int)v3.y + p2.Y), new Emgu.CV.Structure.MCvScalar(0, 255, 0), 3);
                        }
                    }
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
                            List<Point> points1 = [];
                            for (int k = 0; k < 6; k++)
                                points1.Add(approx[(j + k + 1) % count]);
                            LPositions.Add((new() { center = new(p2.X, p2.Y), forward = v3, point2D = [.. points1] }, i, length));
                        }
                    }
                    // LPositions.Add((new(centerX, centerY), i, length));
                }
            }
            // for (int j = 0; j < approx.Size; j++)
            //     CvInvoke.Circle(image, approx[j], 1, new Emgu.CV.Structure.MCvScalar(0, 255, 0));
        }
        foreach (var (lightBar, index, length) in LPositions)
        {
            if (FindNearestPoint(lightBar.center, vectorOfPoint, length / 3))
            {
                var tmpLight = lightBar;
                tmpLight.type = LLightBarType.ForwardShort;
                bar.Add(tmpLight);
                // CvInvoke.DrawContours(image, contours, index, new Emgu.CV.Structure.MCvScalar(255, 255, 0), 2);
            }
            else
            {
                var tmpLight = lightBar;
                tmpLight.type = LLightBarType.ForwardLong;
                bar.Add(tmpLight);
                // CvInvoke.DrawContours(image, contours, index, new Emgu.CV.Structure.MCvScalar(0, 255, 0), 2);}
            }

        }
        var list = lLightBarFilter.FilterLightBar(bar);
        edgesPub.LoadInstance(ref edges);
        blurredPub.LoadInstance(ref blurred);
        if (list.Count < 1)
            return;
        // Math.Clamp(2 * list.Count + 2, 4, int.MaxValue)
        List<(Point Point2D, MCvPoint3D32f Point3D)> points = MaxDistanceSampler.Sample(list, list.Count * 6);

        List<(MCvPoint3D32f point3dInCamera, MCvPoint3D32f Point3dInWorld)> point3dPairs = new();
        IntPtr vertexData = pc.Instance.Value.VertexData;
        int vertexCount = pc.Instance.Value.Count;

        foreach ((var _2d, var _3d) in points)
        {
            int index = _2d.Y * image.Width + _2d.X;
            if (index > vertexCount)
                continue;
            IntPtr ptr = IntPtr.Add(vertexData, index * Marshal.SizeOf(typeof(Vertex)));
            var tmp = Marshal.PtrToStructure<Vertex>(ptr);
            MCvPoint3D32f tmpVec = new(tmp.X * 1000, tmp.Y * 1000, tmp.Z * 1000);
            if (tmpVec.Norm != 0)
                point3dPairs.Add((tmpVec, _3d));
        }
        point3dPairs = PointCloudFilter.RemoveOutliers(point3dPairs);
        var (translation, quaternion) = ICPSolver.ICP(point3dPairs);
        DrawCameraCoordinateSystem(translation, quaternion, ref image);
        // solver.Solve(points, image);
        approxPub.LoadInstance(ref image);
    }

    [StructLayout(LayoutKind.Sequential)] public struct Vertex { public float X; public float Y; public float Z; }
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

    static MCvPoint3D32f DeprojectPixelToPoint(Intrinsics intrinsics, PointF pixel, float depth)
    {
        float x = (pixel.X - intrinsics.ppx) / intrinsics.fx;
        float y = (pixel.Y - intrinsics.ppy) / intrinsics.fy;
        return new(x * depth, y * depth, depth);
    }

    public static void DrawCameraCoordinateSystem(Vector3d translation, Quaterniond quaternion, ref Mat image)
    {

        // 计算旋转矩阵
        var rotationMatrix = quaternion.ToRotationMatrix();

        // 定义相机坐标系中的原点
        Vector3d cameraOrigin = -translation;
        cameraOrigin = rotationMatrix.Transpose() * cameraOrigin;
        // 定义坐标轴长度
        double axisLength = 100.0;

        // 计算坐标轴终点位置
        Vector3d xEnd = cameraOrigin + rotationMatrix.Transpose() * new Vector3d(axisLength, 0, 0);
        Vector3d yEnd = cameraOrigin + rotationMatrix.Transpose() * new Vector3d(0, axisLength, 0);
        Vector3d zEnd = cameraOrigin + rotationMatrix.Transpose() * new Vector3d(0, 0, axisLength);
        Point center = new(image.Width / 2, image.Height / 2);
        // 投影到图像平面 (简单起见，只保留X和Y轴，忽略Z轴)
        Point origin = new((int)cameraOrigin.x + center.X, (int)cameraOrigin.y + center.Y);
        Point xAxis = new((int)xEnd.x + center.X, (int)xEnd.y + center.Y);
        Point yAxis = new((int)yEnd.x + center.X, (int)yEnd.y + center.Y);
        Point zAxis = new((int)zEnd.x + center.X, (int)zEnd.y + center.Y);

        // 绘制坐标轴
        CvInvoke.Line(image, origin, xAxis, new MCvScalar(0, 0, 255), 2);  // X轴红色
        CvInvoke.Line(image, origin, yAxis, new MCvScalar(0, 255, 0), 2);  // Y轴绿色
        CvInvoke.Line(image, origin, zAxis, new MCvScalar(255, 0, 0), 2);  // Z轴蓝色

    }
    public static class PointCloudFilter
    {
        public static List<(MCvPoint3D32f point3dInCamera, MCvPoint3D32f Point3dInWorld)> RemoveOutliers(
            List<(MCvPoint3D32f point3dInCamera, MCvPoint3D32f Point3dInWorld)> points)
        {
            if (points == null || points.Count == 0)
                return new List<(MCvPoint3D32f point3dInCamera, MCvPoint3D32f Point3dInWorld)>();

            // 获取Z轴值
            var zValues = points.Select(p => p.point3dInCamera.Z).ToList();

            // 计算IQR
            double q1 = GetPercentile(zValues, 25);
            double q3 = GetPercentile(zValues, 75);
            double iqr = q3 - q1;

            // 定义上下限
            double lowerBound = q1 - 1 * iqr;
            double upperBound = q3 + 1 * iqr;

            // 过滤离群点
            var filteredPoints = points.Where(p => p.point3dInCamera.Z >= lowerBound && p.point3dInCamera.Z <= upperBound).ToList();

            return filteredPoints;
        }

        private static double GetPercentile(List<float> sortedValues, double percentile)
        {
            if (sortedValues == null || sortedValues.Count == 0)
                throw new ArgumentException("sortedValues cannot be null or empty");

            sortedValues.Sort();
            int n = sortedValues.Count;
            double rank = percentile / 100.0 * (n - 1);
            int lowerIndex = (int)rank;
            int upperIndex = Math.Min(lowerIndex + 1, n - 1);
            double weight = rank - lowerIndex;

            return sortedValues[lowerIndex] * (1 - weight) + sortedValues[upperIndex] * weight;
        }
    }

}