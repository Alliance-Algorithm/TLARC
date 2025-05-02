namespace Maps;

interface IESDF : IGridMap
{
  public double this[Vector3i index] { get; }
  public Vector3d Gradient(Vector3d position);
}
