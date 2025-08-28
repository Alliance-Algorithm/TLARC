using System.Numerics;

namespace Kernel.DataInterfaces.Visualization;

public interface ICircle : IShape
{
    float R { get; }
    Vector2 Origin { get; }
}