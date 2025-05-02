using Maps;

namespace ALPlanner.Collider;

class SemanticMapCollider : Component, ICollider
{
  IGridMap map;
  Transform sentry;
  public Vector3d Position { get; private set; }
  public Vector3d Velocity { get; private set; }

  public override void Start() => Position = sentry.Position;

  public override void Update()
  {
    Velocity = new();
    if (map.CheckAccessibility(Position, sentry.Position))
    {
      Velocity = (sentry.Position - Position) / DeltaTimeInSecond;
      Position = sentry.Position;
    }
  }
}
