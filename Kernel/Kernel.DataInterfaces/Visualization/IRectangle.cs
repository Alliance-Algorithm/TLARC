using System.Numerics;
using Kernel.DataInterfaces.Constraints;

namespace Kernel.DataInterfaces.Visualization;

public interface IRectangle : IShape, IConstraint
{
    /// <summary>
    /// 矩形在世界中的右下角位置
    /// </summary>
    public Vector2 Origin { get; }
    public Vector2 Size { get; }
    public double RotationRad { get; }
}