using System.Numerics;

namespace Kernel.Contract.Geometry;

public struct Pose : ITlarcData
{
    public Header       Header;
    public Vector3      Position;
    public Quaternion   Orientation;
}