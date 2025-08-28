
using System.Numerics;
using Kernel.DataInterfaces.Constraints;
using Kernel.DataInterfaces.Visualization;
namespace Kernel.DataInterfaces.Constraints;

public record Circle2D(float R, Vector2 Origin) : IConstraint, ICircle;