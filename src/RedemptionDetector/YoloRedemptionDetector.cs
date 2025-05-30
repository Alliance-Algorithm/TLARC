using System;
using System.Drawing;
using System.Runtime.InteropServices;
using AutoExchange.RedemptionDetector.Utils;
using Emgu.CV;
using Emgu.CV.Structure;
using Intel.RealSense;
using TlarcKernel.IO.ProcessCommunicateInterfaces;

namespace AutoExchange.RedemptionDetector;

class YoloRedemptionDetector : Component, IRedemptionDetector
{
  public ReadOnlyUnmanagedSubscription<Points> pointCloudSub = new("/real_sense/frame/pc");
  public ReadOnlyUnmanagedSubscription<Mat> depthSub = new("/real_sense/depth");
  public ReadOnlyUnmanagedSubscription<Mat> rawImage = new("/image/raw");
  public ReadOnlyUnmanagedInterfacePublisher<Mat> approxPub = new("/image/approx");
  string modelPath = "onnxModel/redemption.onnx";
  bool IsBlue = false;
  OnnxYoloHelper predictor;
  KeyPointHelper keypointHelper;

  (Vector3d position, Quaterniond rotation) translate;
  public (Vector3d position, Quaterniond rotation) redemptionInCamera;

  public override void Start()
  {
    predictor = new(TlarcSystem.RootPath + modelPath, IsBlue);

    LLightBar[] lightBars =
    [
      new()
      {
        center = new(),
        point3D =
        [
          new(0, -126.5f, 126.5f),
          new(0, -87.5f, 126.5f),
          new(0, -87.5f, 137.5f),
          new(0, -137.5f, 137.5f),
          new(0, -137.5f, 87.5f),
          new(0, -126.5f, 87.5f),
        ],
      },
      new()
      {
        center = new(),
        point3D =
        [
          new(0, 126.5f, 126.5f),
          new(0, 126.5f, 87.5f),
          new(0, 137.5f, 87.5f),
          new(0, 137.5f, 137.5f),
          new(0, 87.5f, 137.5f),
          new(0, 87.5f, 126.5f),
        ],
      },
      new()
      {
        center = new(),
        point3D =
        [
          new(0, 126.5f, -126.5f),
          new(0, 87.5f, -126.5f),
          new(0, 87.5f, -137.5f),
          new(0, 137.5f, -137.5f),
          new(0, 137.5f, -87.5f),
          new(0, 126.5f, -87.5f),
        ],
      },
      new()
      {
        center = new(),
        point3D =
        [
          new(0, -126.5f, -126.5f),
          new(0, -126.5f, -87.5f),
          new(0, -137.5f, -87.5f),
          new(0, -137.5f, -137.5f),
          new(0, -87.5f, -137.5f),
          new(0, -87.5f, -126.5f),
        ],
      },
      new()
      {
        center = new(),
        point3D =
        [
          new(59.642136f, -144f, 0),
          new(145.5f, -144f, -100f),
          new(145.5f, -144f, -85.857864f),
          new(45.5f, -144f, 0f),
          new(145.5f, -144f, 85.857864f),
          new(145.5f, -144f, 100f),
        ],
      },
      new()
      {
        center = new(),
        point3D =
        [
          new(59.642136f, 144f, 0),
          new(145.5f, 144f, 100f),
          new(145.5f, 144f, 85.857864f),
          new(45.5f, 144, 0f),
          new(145.5f, 144f, -85.857864f),
          new(145.5f, 144f, -100f),
        ],
      },
    ];

    keypointHelper = new KeyPointHelper(lightBars);
  }

  public override void Update()
  {
    using var raw = rawImage.Rent;
    if (raw == null)
      return;
    using var pc = pointCloudSub.Rent;
    if (pc == null)
      return;
    Mat image = raw.Instance.Value.Clone();

    Mat[] channels = image.Split();
    using Mat blueChannel = channels[0] - channels[2];
    using Mat redChannel = channels[2] - channels[0];
    channels[0].Dispose();
    channels[1].Dispose();
    channels[2].Dispose();

    var output = predictor.ProcessImage(redChannel);

    var pointPairs = keypointHelper.UpdateKeyPoints(output);
    keypointHelper.DrawDetections(image, output);
    // predictor.DrawDetections(image, output);

    List<(MCvPoint3D32f point3dInCamera, MCvPoint3D32f Point3dInWorld)> point3dPairs = new();
    IntPtr vertexData = pc.Instance.Value.VertexData;
    int vertexCount = pc.Instance.Value.Count;

    foreach ((var _2d, var _3d) in pointPairs)
    {
      int index = _2d.Y * image.Width + _2d.X;
      if (index >= vertexCount || index < 0)
        continue;
      IntPtr ptr = IntPtr.Add(vertexData, index * Marshal.SizeOf(typeof(Vertex)));
      var tmp = Marshal.PtrToStructure<Vertex>(ptr);
      MCvPoint3D32f tmpVec = new(tmp.X * 1000, tmp.Y * 1000, tmp.Z * 1000);
      if (tmpVec.Norm != 0)
        point3dPairs.Add((tmpVec, _3d));
    }

    translate = ICPSolver.ICP(point3dPairs);
    redemptionInCamera = (
      -(translate.rotation.ToRotationMatrix().Transpose() * translate.position) / 1000,
      translate.rotation.ToRotationMatrix().Transpose().ToQuaternion()
    );
    DrawCameraCoordinateSystem(redemptionInCamera.position, redemptionInCamera.rotation, image);

    approxPub.LoadInstance(ref image);
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct Vertex
  {
    public float X;
    public float Y;
    public float Z;
  }

  static void DrawCameraCoordinateSystem(Vector3d translation, Quaterniond quaternion, Mat image)
  {
    // 计算旋转矩阵
    var rotationMatrix = quaternion.ToRotationMatrix();

    // 定义相机坐标系中的原点
    Vector3d cameraOrigin = translation;

    // 定义坐标轴长度
    double axisLength = 0.1;

    // 计算坐标轴终点位置
    Vector3d xEnd = cameraOrigin + rotationMatrix * new Vector3d(axisLength, 0, 0);
    Vector3d yEnd = cameraOrigin + rotationMatrix * new Vector3d(0, axisLength, 0);
    Vector3d zEnd = cameraOrigin + rotationMatrix * new Vector3d(0, 0, axisLength);

    // 投影到图像平面
    Point origin = ProjectPoint(cameraOrigin);
    Point xAxis = ProjectPoint(xEnd);
    Point yAxis = ProjectPoint(yEnd);
    Point zAxis = ProjectPoint(zEnd);

    // 绘制坐标轴
    CvInvoke.Line(image, origin, xAxis, new MCvScalar(0, 0, 255), 2); // X轴红色
    CvInvoke.Line(image, origin, yAxis, new MCvScalar(0, 255, 0), 2); // Y轴绿色
    CvInvoke.Line(image, origin, zAxis, new MCvScalar(255, 0, 0), 2); // Z轴蓝色
  }

  static Point ProjectPoint(Vector3d point)
  {
    // 提取相机内参
    double fx = 642.5304;
    double fy = 641.76654;
    double cx = 652.053;
    double cy = 370.06146;

    // 投影到图像平面
    double x = (fx * point.x / point.z) + cx;
    double y = (fy * point.y / point.z) + cy;

    return new Point((int)x, (int)y);
  }

  public (Vector3d position, Quaterniond rotation) GetRedemptionInCamera() => redemptionInCamera;
}
