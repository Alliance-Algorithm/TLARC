using BEPUphysics.BroadPhaseEntries.Events;

namespace Maps;

static class OccupancyGridMapBuilder
{
  public static OccupancyMapData Init(string path)
  {
    if (path.EndsWith("tlm"))
    {
      return Internal_Init(path);
    }
    else
    {
      return Internal_Init();
    }
  }

  private static OccupancyMapData Internal_Init(string tlm_path)
  {
    throw new NotImplementedException();
  }

  private static OccupancyMapData Internal_Init()
  {
    return new OccupancyMapData(101, 101, 0.1f);
  }
}
