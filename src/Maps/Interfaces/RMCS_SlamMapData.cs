namespace Maps.Interfaces;

internal class DynamicMapData
{
  public sbyte[,] _map;
  public int offsetX;
  public int offsetY;
  public (double Sin, double Cos) forward;
}
