using System.Numerics;

namespace Kernel.DataInterfaces.Navigation;

public interface IObstacle : ITlarcData
{
    /// <summary>
    /// 从from 周围radius 范围内的所有障碍物
    /// </summary>
    /// <param name="from">起始位置</param>
    /// <param name="to">终点位置</param>
    /// <returns>如果有可以直线通过：true</returns>
    public bool FindObstacle(Vector2 from, float radius, out IEnumerable<Vector2> obstacles);
}