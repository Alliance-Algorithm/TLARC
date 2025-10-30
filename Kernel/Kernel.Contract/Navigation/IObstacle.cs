using System.Numerics;

namespace Kernel.Contract.Navigation;

public interface IObstacle : ITlarcData
{
    /// <summary>
    /// 从from 周围radius 范围内的最近障碍物距离
    /// </summary>
    /// <returns>当前点不为障碍物为return</returns>
    public bool SearchNearest(Vector2 from, float radius, out float distance);
}