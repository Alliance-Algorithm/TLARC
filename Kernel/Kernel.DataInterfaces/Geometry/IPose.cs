using System.Numerics;

namespace Kernel.DataInterfaces.Geometry;

public interface IPose : ITlarcData
{
    public Vector3 Position { get; }
    public Quaternion Orientation { get; }
}