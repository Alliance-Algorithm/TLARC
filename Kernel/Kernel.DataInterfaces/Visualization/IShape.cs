using g4;

namespace Kernel.DataInterfaces.Visualization;

public interface IShape
{
    /// <summary>
    /// 圆形中心点
    /// </summary>
    Vector2d Center { get; }
    /// <summary>
    /// 圆形半径
    /// </summary>
    Vector2d Radius { get; }
}
