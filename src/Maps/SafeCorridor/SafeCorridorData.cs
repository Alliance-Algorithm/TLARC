using System.Collections;
using Accord.IO;

namespace Maps;
readonly struct Rectangle(double minX, double minY, double maxX, double maxY, double[,] rotation)
{

  public readonly double MinY => minY;
  public readonly double MinX => minX;
  public readonly double MaxY => maxY;
  public readonly double MaxX => maxX;

  public readonly double[,] Rotation = rotation;

  public bool Check(Vector3d position)
  {
    double[] xy1 = [position.x, position.y];
    double[] minXY1 = [MinX, MinY];
    double[] maxXY1 = [MaxX, MaxY];
    var xy = Rotation.Dot(xy1);
    var minXY = Rotation.Dot(minXY1);
    var maxXY = Rotation.Dot(maxXY1);

    return xy[0] >= minXY[0] && xy[1] >= minXY[1] && xy[0] <= maxXY[0] && xy[1] <= maxXY[1];
  }
}

class SafeCorridorData : IEnumerable<Rectangle>
{
  public void PushIn(in Rectangle rectangle) => rectangles.Add(rectangle);
  List<Rectangle> rectangles = [];

  public int Count => rectangles.Count;
  public Rectangle this[int index] => rectangles[index];

  public IEnumerator<Rectangle> GetEnumerator()
  {
    int i = 0;
    while (i < rectangles.Count)
      yield return rectangles[i++];
  }

  IEnumerator IEnumerable.GetEnumerator()
  {
    return GetEnumerator();
  }
}
