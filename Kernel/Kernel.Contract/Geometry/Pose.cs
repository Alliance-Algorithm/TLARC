using System.Numerics;

namespace Kernel.Contract.Geometry;

public struct Pose : ITlarcData
{
    public Header       Header;
    public Vector3      Translation;
    public Quaternion   Orientation;
}
public struct Pose2D : ITlarcData
{
    public Header       Header;
    public Vector2      Translation;
    public float        Orientation;
}