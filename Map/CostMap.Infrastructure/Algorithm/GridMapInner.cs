using System.Numerics;
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
    internal static bool CheckMoveable(Vector2        target,
                                       IGridMap2DData data,
                                       sbyte          threshold,
                                       ThresholdType  type)
    {
        var vecInWorld = target - data.Origin;
        var vecInMap   = (data.RotationMatrix * Matrix3x2.CreateTranslation(vecInWorld / data.Resolution)).Translation;
        if (vecInMap.X < 0 || vecInMap.X >= data.Width || vecInMap.Y < 0 || vecInMap.Y >= data.Height)
            return false;

        int xIndexInMap = (int)vecInMap.X,
            yIndexInMap = (int)vecInMap.Y;
        return type switch
        {
            ThresholdType.Equal        => data.Data[xIndexInMap + yIndexInMap * data.Width] == threshold,
            ThresholdType.GreaterEqual => data.Data[xIndexInMap + yIndexInMap * data.Width] >= threshold,
            ThresholdType.LessEqual    => data.Data[xIndexInMap + yIndexInMap * data.Width] <= threshold,
            ThresholdType.Less         => data.Data[xIndexInMap + yIndexInMap * data.Width] < threshold,
            ThresholdType.Greater      => data.Data[xIndexInMap + yIndexInMap * data.Width] > threshold,
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
    internal static bool CheckMoveable(Vector2        from,
                                       Vector2        to,
                                       IGridMap2DData data,
                                       sbyte          threshold,
                                       ThresholdType  type)
    {
        var fromVecInWorld = from - data.Origin;
        var fromVecInMap = (data.RotationMatrix *
                            Matrix3x2.CreateTranslation(fromVecInWorld / data.Resolution))
            .Translation;
        if (fromVecInMap.X < 0 || fromVecInMap.X >= data.Width || fromVecInMap.Y < 0 || fromVecInMap.Y >= data.Height)
            return false;
        var toVecInWorld = to - data.Origin;
        var toVecInMap = (data.RotationMatrix *
                          Matrix3x2.CreateTranslation(fromVecInWorld / data.Resolution))
            .Translation;
        if (toVecInMap.X < 0 || toVecInMap.X >= data.Width || toVecInMap.Y < 0 || toVecInMap.Y >= data.Height)
            return false;

        Vector2i
            fromIndexInMap = new((int)fromVecInMap.X, (int)fromVecInMap.Y),
            toIndexInMap   = new((int)toVecInMap.X, (int)toVecInMap.Y);


        var indexes = Geometry.ThickLine(fromIndexInMap, toIndexInMap);

        return indexes.All(predicate: indexInMap => type switch
        {
            ThresholdType.Equal        => data.Data[indexInMap.x + indexInMap.y * data.Width] == threshold,
            ThresholdType.GreaterEqual => data.Data[indexInMap.x + indexInMap.y * data.Width] >= threshold,
            ThresholdType.LessEqual    => data.Data[indexInMap.x + indexInMap.y * data.Width] <= threshold,
            ThresholdType.Less         => data.Data[indexInMap.x + indexInMap.y * data.Width] < threshold,
            ThresholdType.Greater      => data.Data[indexInMap.x + indexInMap.y * data.Width] > threshold,
            _                          => false
        });
    }
}