using Maps;

namespace ALPlanner.Collider;

class GridMapCollider : Component, ICollider
{
  [ComponentReferenceFiled]
  IGridMap map;
  Transform sentry;
  public Vector3d Position { get; private set; } = new(-7.5, 0, 0);
  public Vector3d Velocity { get; private set; } = new(-7.5, 0, 0);

  public override void Start() { }

  public override void Update()
  {
    Velocity = new();
    if (map.CheckAccessibility(sentry.Position))
    {
      Velocity = (sentry.Position - Position) / DeltaTimeInSecond;
      Position = sentry.Position;
    }
  }
}
