using System.Buffers;

namespace Maps;

class OccupancyMapData(int sizeX, int sizeY, float resolution) : IGridMap, IDisposable
{
  const float lo_occ = 5f;
  const float lo_free = -6f;
  const sbyte lo_max = 90;
  const sbyte lo_min = 10;

  readonly public int SizeX = sizeX;
  readonly public int SizeY = sizeY;
  readonly public float Resolution = resolution;

  TlarcArray<sbyte> data = new(sizeX, sizeY);

  sbyte this[int x, int y] => data[x, y];

  Vector3i IGridMap.Index => throw new NotImplementedException();

  Vector3d IGridMap.OriginInWorld => Vector3d.Zero;

  Vector3i IGridMap.Size => new(SizeX, SizeY, 1);

  public bool CheckAccessibility(Vector3d from, Vector3d to, float value)
  {
    var length = (int)Math.Round((to - from).Length / Resolution, MidpointRounding.ToZero);
    var dir = (to - from).Normalized * Resolution;

    for (int i = 0; i < length; i++)
    {
      var pos = InternalPositionInWorldToIndex(to - i * dir);

      if (data[pos.x, pos.y] < 50)
        return false;
    }
    return true;
  }

  public bool CheckAccessibility(Vector3i index, float value) => data[index.x, index.y] > 50;
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
    value -= 50;
    if (value > 0)
      data[x, y] = (sbyte)
        Math.Clamp(
          Math.Round(data[x, y] * (1 + (value * lo_occ / 100)), MidpointRounding.AwayFromZero),
          1,
          100
        );
    else
      data[x, y] = (sbyte)
        Math.Clamp(
          Math.Round(data[x, y] * (1 + (value * lo_free / 100)), MidpointRounding.ToZero),
          1,
          100
        );
  }

  public void Dispose() => data.Dispose();

}
