
using System.Drawing;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.Util;
using g4;
using Intel.RealSense;
using Intel.RealSense.Math;
using TlarcKernel.IO.ProcessCommunicateInterfaces;

namespace AutoExchange.ExchangeStationDetector;

class Redemption : Component
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
    public Vector2d thresholdMinMax = new() { x = 80, y = 255 };
    List<LampInfo> lampInfos = [];

    public override void Start()
    {
        thresholdMinMax.y = Math.Clamp(thresholdMinMax.y, 0, 255);
        thresholdMinMax.x = Math.Clamp(thresholdMinMax.x, 0, thresholdMinMax.y);
    }

    public static Mat SharpenImage(Mat image)
    {
        Mat sharpened = new Mat(image.Height, image.Width, image.Depth, image.NumberOfChannels);
        using Mat kernel = new Mat(new System.Drawing.Size(3, 3), Emgu.CV.CvEnum.DepthType.Cv64F, 1);
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
        lampInfos = [];
        // Mat image = SharpenImage(raw.Instance.Value.Clone());
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
            CvInvoke.MedianBlur(blueChannel, blurred, 1);
        }
        else
        {

            CvInvoke.MedianBlur(redChannel, blurred, 5);
            using Mat kernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(5, 5), new Point(-1, -1));
            CvInvoke.MorphologyEx(blurred, blurred, MorphOp.Close, kernel, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(1));
        }
        CvInvoke.Canny(blurred, edges, 5, 6000, 5, false);

        using VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
        CvInvoke.FindContours(edges, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);
        List<Vector2d> others = [];
        // edges.Dispose();
        // edges = Mat.Zeros(image.Rows, image.Cols, DepthType.Cv8U, 1);
        // 获取最小外接矩形（ROI）
        if (contours.Size > 0)
        {
            for (int i = 0, k = (int)contours.Size; i < k; i++)
            {
                RotatedRect rotatedRect = CvInvoke.MinAreaRect(contours[i]);
                PointF[] vertices = rotatedRect.GetVertices();
                Point[] points = Array.ConvertAll(vertices, Point.Round);

                for (int j = 0; j < points.Length; j++) { CvInvoke.Line(image, points[j], points[(j + 1) % points.Length], new MCvScalar(0, 255, 0), 2); }
            }
        }


        edgesPub.LoadInstance(ref edges);
        blurredPub.LoadInstance(ref blurred);
        approxPub.LoadInstance(ref image);
    }
}