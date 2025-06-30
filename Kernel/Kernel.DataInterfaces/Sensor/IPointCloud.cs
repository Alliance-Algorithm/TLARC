using System.Numerics;

namespace Kernel.DataInterfaces.Sensor;

public interface IPointCloud : ITlarcData, IHeader
{
    Vector3[] Points { get; }
}

public class PointCloud : IPointCloud
{
    public required Vector3[] Points { get; init; }
    public required string Identifier { get; set; }
}