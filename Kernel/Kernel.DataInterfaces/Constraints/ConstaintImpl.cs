
using System.Numerics;
using Kernel.DataInterfaces.Constraints;
using Kernel.DataInterfaces.Visualization;
namespace Kernel.DataInterfaces.Constraints;

public readonly record struct Circle2D(float R, Vector2 Origin) : IConstraint, ICircle;

/// <summary>
/// Axis-aligned bounding boxes
/// </summary>
public readonly struct AABB2D(float MinX, float MinY, float MaxX, float MaxY) : IConstraint, IRectangle
{
    public readonly Vector2 Origin { get; } = new(MinX, MinY);

    public readonly Vector2 Size { get; } = new(MaxX - MinX, MaxY - MinY);

    public readonly double RotationRad { get; } = 0;
}