using System.Numerics;

namespace Kernel.DataInterfaces.Sensor;

public interface IPointCloud
{
    Vector3[] Points { get; set; }
}