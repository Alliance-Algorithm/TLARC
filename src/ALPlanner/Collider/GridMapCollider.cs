
using Maps;

namespace ALPlanner.Collider;

class GridMapCollider : Component, ICollider
{
    [ComponentReferenceFiled]
    IGridMap map;
    Transform sentry;
    public Vector3d Position { get; private set; }

    public override void Start() => Position = sentry.Position;
    public override void Update()
    {
        if (map.CheckAccessibility(new(), map.PositionInWorldToIndex(sentry.Position)))
            Position = sentry.Position;
    }
}