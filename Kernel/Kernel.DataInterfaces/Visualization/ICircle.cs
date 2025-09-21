using System.Numerics;
using Kernel.DataInterfaces.Constraints;

namespace Kernel.DataInterfaces.Visualization;

public interface ICircle : IShape, IConstraint
{
    float R { get; }
    Vector2 Origin { get; }
}