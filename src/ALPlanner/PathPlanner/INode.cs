using g4;

namespace ALPlanner.PathPlanner.Nodes;

internal interface INode
{
    public Vector3d PositionInWorld { get; }
    public INode? Parent { get; set; }
    public IEnumerable<INode> Children { get; }
    public float TotalCost { get; }
    public bool GeometricallyEqualTo(INode node);
}