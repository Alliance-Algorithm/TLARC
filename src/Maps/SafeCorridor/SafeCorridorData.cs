using System.Collections;
using Accord.IO;

namespace Maps;
readonly struct Rectangle(double x, double y, double xLength, double yLength)
{
  private readonly double x = x;
  private readonly double y = y;
  private readonly double XLength = xLength;
  private readonly double YLength = yLength;

  public readonly Vector2d Center => new(x, y);
  public readonly double MinY => y - YLength / 2;
  public readonly double MinX => x - XLength / 2;
  public readonly double MaxY => y + YLength / 2;
  public readonly double MaxX => x + XLength / 2;

}

class SafeCorridorData : IEnumerable<Rectangle>
{
  public void PushIn(in Rectangle rectangle) => rectangles.Add(rectangle);
  List<Rectangle> rectangles;

  public double Count => rectangles.Count;
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
