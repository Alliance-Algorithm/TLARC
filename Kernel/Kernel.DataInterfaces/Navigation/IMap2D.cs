using System.Numerics;

namespace Kernel.DataInterfaces.Navigation;

public interface IMap2D : ITlarcData
{
    /// <summary>
    /// 从from 到 to 是否可以移动
    /// </summary>
    /// <param name="from">起始位置</param>
    /// <param name="to">终点位置</param>
    /// <returns>如果有可以直线通过：true</returns>
    public bool IsMoveAble(Vector2 from, Vector2 to);

    /// <summary>
    /// position是否有障碍物
    /// </summary>
    /// <param name="position">tlarc坐标系坐标</param>
    /// <returns>如果有障碍物：true</returns>
    public bool IsMoveAble(Vector2 position);
}