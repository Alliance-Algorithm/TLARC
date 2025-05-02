namespace Maps;

internal class ESDFMapData
{
  public sbyte[,] Map => _staticMap;
  public int[,,] Obstacles => _staticObs;
  public int SizeX => _size_x;
  public int SizeY => _size_y;
  public float Resolution => _resolution;

  public int _size_x;
  public int _size_y;
  public float _resolution;
  public sbyte[,] _staticMap;
  public int[,,] _staticObs;
}
