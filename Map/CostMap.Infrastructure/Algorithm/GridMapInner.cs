using g4;
using Kernel.DataInterfaces.Navigation;

namespace CostMap.Infrastructure.Algorithm;

internal class GridMapInner
{
    internal enum ThresholdType
    {
        Greater,
        Less,
        GreaterEqual,
        LessEqual,
        Equal
    }


    /// <summary>
    /// 返回目标点是否有障碍物
    /// </summary>
    /// <param name="target">目标点</param>
    /// <param name="data">输入地图数据</param>
    /// <param name="threshold">阈值,与之作比较</param>
    /// <param name="type">比较方法
    ///<para>example: type == LessEqual then if(data &le; threshold) return true;</para>
    /// </param>
    /// <returns>如果有障碍物：true</returns>
    internal static bool CheckMoveable(Vector2d       target,
                                       IGridMap2DData data,
                                       sbyte          threshold,
                                       ThresholdType  type)
    {
        var vecInWorld = target - data.Origin;
        var vecInMap   = data.RotationMatrix * vecInWorld / data.Resolution;
        if (vecInMap.x < 0 || vecInMap.x >= data.Size.x || vecInMap.y < 0 || vecInMap.y >= data.Size.y)
            return false;

        var indexInMap = new Vector2i((int)vecInMap.x, (int)vecInMap.y);
        return type switch
        {
            ThresholdType.Equal        => data.Data[indexInMap.x + indexInMap.y * data.Size.x] == threshold,
            ThresholdType.GreaterEqual => data.Data[indexInMap.x + indexInMap.y * data.Size.x] >= threshold,
            ThresholdType.LessEqual    => data.Data[indexInMap.x + indexInMap.y * data.Size.x] <= threshold,
            ThresholdType.Less         => data.Data[indexInMap.x + indexInMap.y * data.Size.x] < threshold,
            ThresholdType.Greater      => data.Data[indexInMap.x + indexInMap.y * data.Size.x] > threshold,
            _                          => false
        };
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="from">起始位置</param>
    /// <param name="to">终点位置</param>
    /// <param name="data">输入地图数据</param>
    /// <param name="threshold">阈值,与之作比较</param>
    /// <param name="type">比较方法
    ///<para>example: type == LessEqual then if(data &le; threshold) return true;</para>
    /// </param>
    /// <returns>如果有障碍物：true</returns>
    internal static bool CheckMoveable(Vector2d       from,
                                       Vector2d       to,
                                       IGridMap2DData data,
                                       sbyte          threshold,
                                       ThresholdType  type)
    {
        var fromVecInWorld = from - data.Origin;
        var fromVecInMap   = data.RotationMatrix * fromVecInWorld / data.Resolution;
        if (fromVecInMap.x < 0 || fromVecInMap.x >= data.Size.x || fromVecInMap.y < 0 || fromVecInMap.y >= data.Size.y)
            return false;
        var toVecInWorld = to - data.Origin;
        var toVecInMap   = data.RotationMatrix * toVecInWorld / data.Resolution;
        if (toVecInMap.x < 0 || toVecInMap.x >= data.Size.x || toVecInMap.y < 0 || toVecInMap.y >= data.Size.y)
            return false;

        var fromIndexInMap = new Vector2i((int)fromVecInMap.x, (int)fromVecInMap.y);
        var toIndexInMap   = new Vector2i((int)toVecInMap.x,   (int)toVecInMap.y);


        var indexes = Geometry.ThickLine(fromIndexInMap, toIndexInMap);

        return indexes.All(predicate: indexInMap => type switch
        {
            ThresholdType.Equal        => data.Data[indexInMap.x + indexInMap.y * data.Size.x] == threshold,
            ThresholdType.GreaterEqual => data.Data[indexInMap.x + indexInMap.y * data.Size.x] >= threshold,
            ThresholdType.LessEqual    => data.Data[indexInMap.x + indexInMap.y * data.Size.x] <= threshold,
            ThresholdType.Less         => data.Data[indexInMap.x + indexInMap.y * data.Size.x] < threshold,
            ThresholdType.Greater      => data.Data[indexInMap.x + indexInMap.y * data.Size.x] > threshold,
            _                          => false
        });
    }
}
