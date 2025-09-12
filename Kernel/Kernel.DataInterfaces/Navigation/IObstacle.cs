using System.Numerics;

namespace Kernel.DataInterfaces.Navigation;

public interface IObstacle : ITlarcData
{
    IHeader Header { get; }
    /// <summary>
    /// 从from 周围radius 范围内的最近障碍物距离
    /// </summary>
    /// <returns>如果有可以直线通过：true</returns>
    public bool FindNearestObstacleDistance(Vector2 from, float radius, out float obstacles);
}