using System.Collections;
using ALPlanner.TrajectoryOptimizer;
using Emgu.CV.Dpm;

namespace Maps;

class SafeCorridor : Component, IMap, IEnumerable<Rectangle>
{
  SafeCorridorData data;

  [ComponentReferenceFiled]
  ISafeCorridorGenerator generator;

  public bool CheckAccessibility(Vector3d from, Vector3d to, float value = 0)
  {
    return data.Any(x => x.MaxX > from.x && x.MinX < from.x && x.MaxX > to.x && x.MinX < to.x &&
                        x.MaxY > from.y && x.MinY < from.y && x.MaxY > to.y && x.MinY < to.y);
  }

  public bool CheckAccessibility(Vector3d index, float value = 0)
  {
    return data.Any(x => x.MaxX > index.x && x.MinX < index.x && x.MaxY > index.y && x.MinY < index.y);
  }

  public void Generate(Vector3d[] pointLists)
  {
    data = generator.Generate(pointLists);
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

      Constraint c1 = new(i, data[i].Rotation, min[0], max[0]);
      Constraint c2 = new(i, data[i].Rotation, min[1], max[1]);

      if (constantX is null)
        constraintCollection.XBegin = c1;
      else constantX.next = c1;
      if (constantY is null)
        constraintCollection.YBegin = c2;
      else constantY.next = c2;

      constantX = c1;
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
}
