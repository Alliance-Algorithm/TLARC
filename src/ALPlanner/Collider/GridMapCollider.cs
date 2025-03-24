
using Maps;

namespace ALPlanner.Collider;

class GridMapCollider : Component, ICollider
{
    [ComponentReferenceFiled]
    IGridMap map;
    Transform sentry;
    public Vector3d Position { get; private set; } = new(-7.5, 0, 0);

    public override void Start() { }
    public override void Update()
    {
        if ((Position - sentry.Position).LengthSquared > 1 || map.CheckAccessibility(Position, sentry.Position))
            if (map.CheckAccessibility(map.PositionInWorldToIndex(sentry.Position))  )
                Position = sentry.Position;
    }
}