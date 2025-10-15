using System.Numerics;
using Kernel.Contract.Constraints;

namespace Kernel.Contract.Visualization;

public struct Rectangle : IShape, IConstraint
{
    /// <summary>
    /// 矩形在世界中的右下角位置
    /// </summary>
    public Vector2  Origin;
    public Vector2  Size;
    public double   RotationRad;
}