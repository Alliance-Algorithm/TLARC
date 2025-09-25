using System.Numerics;

namespace Kernel.DataInterfaces.Navigation;

public interface IObstacle : ITlarcData
{
    IHeader Header { get; }
    /// <summary>
    /// 从from 周围radius 范围内的最近障碍物距离
    /// </summary>
    /// <returns>当前点不为障碍物为return</returns>
    public bool FindNearestObstacleDistance(Vector2 from, float radius, out float obstacles);
}