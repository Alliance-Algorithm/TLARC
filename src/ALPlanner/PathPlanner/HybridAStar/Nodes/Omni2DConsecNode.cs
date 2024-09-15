using System.ComponentModel;
using System.Diagnostics;
using g4;

namespace ALPlanner.PathPlanner.HybridAStar.Nodes;

class Omni2DConsecNode : Component, INode
{
    const double IterationStep = 0.2f;
    public Omni2DConsecNode(Vector3d position)
    {
        PositionInWorld = position;
    }
    public Vector3d PositionInWorld { get; private set; }

    public IEnumerable<INode> Children => throw new NotImplementedException();

    public INode? Parent { get; set; } = null;

    float INode.TotalCost => throw new NotImplementedException();

    public bool GeometricallyEqualTo(INode node)
    {
        return (node.PositionInWorld - PositionInWorld).Length < IterationStep;
    }
}