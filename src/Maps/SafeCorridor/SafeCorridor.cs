using System.Collections;
using ALPlanner.TrajectoryOptimizer;
using Emgu.CV.Dpm;

namespace Maps;

class SafeCorridor : Component, IMap, IEnumerable<Rectangle>
{
  SafeCorridorData data = new();

  [ComponentReferenceFiled]
  ISafeCorridorGenerator generator;
  // OccupancyMapData mapData = new(301, 201, 0.1f);
  private IO.ROS2Msgs.Nav.OccupancyGrid _rosMapPublisher;
  public int Count => data.Count;
  public bool CheckAccessibility(Vector3d from, Vector3d to, float value = 0)
  {
    return data.Any(x =>
    {
      var dir = (to - from).Normalized * 0.1f;
      if (!x.Check(to))
        return false;
      for (var begin = to - dir; (begin - from).Length > 0.2; begin -= dir)
        if (!x.Check(begin))
          return false;
      if (!x.Check(from))
        return false;
      return true;
    }
    );
  }

  public bool CheckAccessibility(Vector3d index, float value = 0)
  {
    return data.Any(x => x.Check(index));
  }

  public int FindIndex(Vector3d last, Vector3d position)
  {
    int index = -1;
    for (int i = 0; i < Count; i++)
    {
      if (CheckAccessibility(last, position))
        index = i;
    }
    return index;
  }
  Vector3d[] _pointLists = [];
  public void Generate(Vector3d[] pointLists, double maxLength)
  {
    _pointLists = pointLists;
    data = generator.Generate(pointLists, maxLength);
  }
  public Vector3d Field(int index)
  {
    if (index < _pointLists.Length)
      return Vector3d.Zero;
    return _pointLists[index] - _pointLists[index + 1];

  }


  public ConstraintCollection GenerateConstraintCollection()
  {
    ConstraintCollection constraintCollection = new();
    Constraint? constantX = null;
    Constraint? constantY = null;
    for (int i = 0; i < data.Count; i++)
    {
      double[] min = [data[i].MinX, data[i].MinY];
      double[] max = [data[i].MaxX, data[i].MaxY];
      min = data[i].Rotation.Dot(min);
      max = data[i].Rotation.Dot(max);

      Constraint c1 = new(i, data[i].Rotation, Math.Min(min[0], max[0]), Math.Max(min[0], max[0]));
      Constraint c2 = new(i, data[i].Rotation, Math.Min(min[1], max[1]), Math.Max(min[1], max[1]));
      if (i == 0 || i == data.Count - 1)
      {
        c1 = new(i, Matrix.Identity(2), _pointLists[i == 0 ? i : i + 1].x, _pointLists[i == 0 ? i : i + 1].x);
        c2 = new(i, Matrix.Identity(2), _pointLists[i == 0 ? i : i + 1].y, _pointLists[i == 0 ? i : i + 1].y);
      }

      if (constantX is null)
        constraintCollection.XBegin = c1;
      else constantX.next = c1;
      if (constantY is null)
        constraintCollection.YBegin = c2;
      else constantY.next = c2;

      constantX = c1;
      constantY = c2;
    }
    constraintCollection.Length = data.Count;
    return constraintCollection;
  }

  public IEnumerator<Rectangle> GetEnumerator()
  {
    return ((IEnumerable<Rectangle>)data).GetEnumerator();
  }

  IEnumerator IEnumerable.GetEnumerator()
  {
    return ((IEnumerable)data).GetEnumerator();
  }
  public override void Start()
  {
    _rosMapPublisher = new(IOManager);
    _rosMapPublisher.RegistryPublisher("debug/safe_corridor");
  }

  public override void Update()
  {
    // for (int i = 0; i < 301; i++)
    //   for (int j = 0; j < 201; j++)
    //     mapData[i, j] = (sbyte)(CheckAccessibility(new(i * 0.1 - 15, j * 0.1 - 10, 0)) ? 100 : 0);

    // _rosMapPublisher.Publish((mapData.ToArray, 0.1f, (uint)mapData.SizeX, (uint)mapData.SizeY));
  }
}
