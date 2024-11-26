using Maps;

namespace ALPlanner.Collider;

class SemanticMapCollider : Component, ICollider
{
    IGridMap map;
    Transform sentry;
    public Vector3d Position { get; private set; }

    public override void Start() => Position = sentry.Position;
    public override void Update()
    {
        if (map.CheckAccessibility(map.PositionInWorldToIndex(Position), map.PositionInWorldToIndex(sentry.Position)))
            Position = sentry.Position;
    }
}