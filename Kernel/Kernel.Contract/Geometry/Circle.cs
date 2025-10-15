using System.Numerics;
using Kernel.Contract.Constraints;

namespace Kernel.Contract.Visualization;

public struct Circle : IShape, IConstraint
{
    public float    R;
    public Vector2  Origin;
}