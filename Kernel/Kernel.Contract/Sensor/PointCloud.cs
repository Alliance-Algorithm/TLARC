using System.Numerics;

namespace Kernel.Contract.Sensor;

public struct PointCloud : ITlarcData
{
    public Header       Header;
    public Vector3[]    Points;
}
