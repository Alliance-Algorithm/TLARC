using System.Numerics;

namespace Kernel.DataInterfaces.Navigation;

public interface ITrajectory2D : ITransform, ITlarcData
{
    /// <summary>
    ///  轨迹起始时间
    /// </summary>
    DateTime FromWhen { get; }

    /// <summary>
    /// 轨迹结束时间
    /// </summary>
    DateTime ToWhen { get; }

    /// <summary>
    /// 获取轨迹中的某个点
    /// </summary>
    /// <param name="time">实际时间</param>
    /// <returns></returns>
    Vector2 GetPosition(DateTime time);

    /// <summary>
    /// 获取一段轨迹点序列
    /// </summary>
    /// <param name="beginTime">起始时间</param>
    /// <param name="stepInSecond">每个点相隔几秒</param>
    /// <param name="count">需要几个点</param>
    /// <returns></returns>
    IEnumerable<Vector2> GetPositionArray(DateTime beginTime, double stepInSecond, int count);
}