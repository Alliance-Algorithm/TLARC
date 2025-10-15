using System.Numerics;
using Kernel.Contract.Constraints;

namespace Kernel.Contract.Visualization;

public struct AABB2D : IShape, IConstraint
{
    public double MaxX;
    public double MaxY;
    public double MinX;
    public double MinY;
}