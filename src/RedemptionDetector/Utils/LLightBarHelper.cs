using System;
using System.Collections.Generic;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace AutoExchange.RedemptionDetector.Utils;

class KeyPointHelper
{
  private LLightBar[] lLightBars;

  public KeyPointHelper(LLightBar[] lightBars)
  {
    lLightBars = lightBars;
  }

  public List<(Point pointInFrame, MCvPoint3D32f point3dInWorld)> UpdateKeyPoints(
    Dictionary<
      int,
      (Rectangle box, List<Point> keypoints, float confidence, float keypointConfidenceSum)
    > detections
  )
  {
    var updatedPoints = new List<(Point, MCvPoint3D32f)>();
    foreach (var detection in detections)
    {
      int classId = detection.Key - 1;
      List<Point> keypoints = detection.Value.keypoints;
      if (classId < lLightBars.Length && classId >= 0)
      {
        lLightBars[classId].point2D = keypoints.ToArray();
        for (int i = 0; i < keypoints.Count; i++)
        {
          updatedPoints.Add(
            (new Point((int)keypoints[i].X, (int)keypoints[i].Y), lLightBars[classId].point3D[i])
          );
        }
      }
    }
    return updatedPoints;
  }

  public void DrawDetections(
    Mat image,
    Dictionary<
      int,
      (Rectangle box, List<Point> keypoints, float confidence, float keypointConfidenceSum)
    > detections
  )
  {
    var classNames = new[] { "Class0", "Class1", "Class2", "Class3", "Class4", "Class5", "Class6" };

    foreach (var detection in detections)
    {
      var (box, keypoints, confidence, keypointConfidenceSum) = detection.Value;

      // 绘制边界框
      CvInvoke.Rectangle(image, box, new MCvScalar(0, 255, 0), 2);

      // 绘制置信度和类别
      string label =
        $"{classNames[detection.Key]}: {confidence:F2} (KP Conf Sum: {keypointConfidenceSum:F2})";
      CvInvoke.PutText(
        image,
        label,
        new Point(box.X, box.Y - 10),
        FontFace.HersheySimplex,
        0.5,
        new MCvScalar(0, 255, 0),
        2
      );

      int i = 0;
      // 绘制关键点
      foreach (var point in keypoints)
      {
        CvInvoke.Circle(
          image,
          new System.Drawing.Point((int)point.X, (int)point.Y),
          5,
          new MCvScalar(255 - i * 40, i++ * 40, 0),
          -1
        );
      }
    }
  }
}

public struct LLightBar
{
  public readonly (Point Point2D, Emgu.CV.Structure.MCvPoint3D32f Point3D) this[int index] =>
    (point2D[index], point3D[index]);
  public Point center;
  public Point[] point2D;
  public MCvPoint3D32f[] point3D;
  public Vector2d forward;
  public LLightBarType type;
}

public enum LLightBarType
{
  Type1,
  Type2,
  Type3,
  Type4,
  Type5,
  Type6,
}
