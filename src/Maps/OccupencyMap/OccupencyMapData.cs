using System.Buffers;
using Accord.IO;

namespace Maps;

class OccupancyMapData : IGridMap, IDisposable
{
  const float lo_occ = 50f;
  const float lo_free = -50f;
  const sbyte lo_max = 90;
  const sbyte lo_min = 10;
  public OccupancyMapData Copy()
  {
    OccupancyMapData tmp = new(SizeX, SizeY, Resolution, data.Clone());
    return tmp;
  }
  public OccupancyMapData(int sizeX, int sizeY, float resolution)
  {
    SizeX = sizeX;
    SizeY = sizeY;
    Resolution = resolution;
    data = new(50, sizeX, sizeY);
  }

  OccupancyMapData(int sizeX, int sizeY, float resolution, in TlarcArray<sbyte> arr)
  {
    SizeX = sizeX;
    SizeY = sizeY;
    Resolution = resolution;
    data = arr;
  }
  public struct Description
  {
    public int SizeX { get; set; }
    public int SizeY { get; set; }
    public float Resolution { get; set; }
  }

  readonly public int SizeX;
  readonly public int SizeY;
  readonly public float Resolution;

  TlarcArray<sbyte> data;
  public sbyte[] ToArray => data.ToArray;

  public sbyte this[int x, int y]
  {
    get => x > 0 && x < SizeX && y > 0 && y < SizeY ? data[x, y] : (sbyte)0;
    set { if (x > 0 && x < SizeX && y > 0 && y < SizeY) data[x, y] = value; }
  }
  Vector3i IGridMap.Index => throw new NotImplementedException();

  Vector3d IGridMap.OriginInWorld => Vector3d.Zero;

  Vector3i IGridMap.Size => new(SizeX, SizeY, 1);

  public bool CheckAccessibility(Vector3d from, Vector3d to, float value)
  {
    var length = (int)Math.Round((to - from).Length / Resolution, MidpointRounding.ToZero);
    var dir = (to - from).Normalized * Resolution;

    for (int i = 0; i < length; i++)
    {
      if (!CheckAccessibility(to - i * dir, 0))
        return false;
    }
    return true;
  }

  public bool CheckAccessibility(Vector3i index, float value) =>
   index.x > 0 && index.x < SizeX && index.y > 0 && index.y < SizeY && data[index.x, index.y] > 76;
  public bool CheckAccessibility(Vector3d index, float value) => CheckAccessibility(InternalPositionInWorldToIndex(index), value);

  Vector3d IGridMap.IndexToPositionInWorld(Vector3i position) =>
    new(
      position.x * Resolution - (SizeX - 1) * Resolution / 2.0f,
      position.y * Resolution - (SizeY - 1) * Resolution / 2.0f,
      0
    );

  Vector3i IGridMap.PositionInWorldToIndex(Vector3d position) =>
    InternalPositionInWorldToIndex(position);

  private Vector3i InternalPositionInWorldToIndex(Vector3d position) =>
    new(
      (int)Math.Round((position.x + ((SizeX - 1) * Resolution / 2.0f)) / Resolution),
      (int)Math.Round((position.y + ((SizeY - 1) * Resolution / 2.0f)) / Resolution),
      0
    );

  internal void Update(int x, int y, sbyte value)
  {
    if (value > 0)
      data[x, y] = 0;
    else 
      data[x, y] = 100;
  }

  public void Dispose() => data.Dispose();

}
