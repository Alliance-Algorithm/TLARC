using System.Diagnostics;
using CostMap.Infrastructure.Data;
using System.Numerics;
using System.Runtime.CompilerServices;
using g4;
using Kernel.Contract.Navigation;
using Kernel.Contract.Sensor;

namespace CostMap.Infrastructure.Algorithm;

public static class OccupancyGridMapBuilder
{
    #region Utils
    private static void AtomicAdd(ref float location, float value)
    {
        var retryCount = 0;
        const int maxRetries = 5;

        while (true)
        {
            // 读取当前位模式
            var currentBits = BitConverter.SingleToInt32Bits(location);
            var current = BitConverter.Int32BitsToSingle(currentBits);

            // 计算新值
            var newValue = Math.Clamp(current + value, -1e10f, 1e10f);
            var newBits = BitConverter.SingleToInt32Bits(newValue);

            // 原子更新
            var actualBits = Interlocked.CompareExchange(
                ref Unsafe.As<float, int>(ref location),
                newBits,
                currentBits
            );

            // 成功更新或达到重试上限
            if (actualBits == currentBits || retryCount++ >= maxRetries)
                break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool CheckBound2D(in Vector3 pos, in GridMap2DData map) =>
        pos is { X: > 0, Y: > 0 } &&
        pos.X < map.Width * map.Resolution &&
        pos.Y < map.Height * map.Resolution;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool CheckBound3D(in Vector3 pos, in Vector3 sensorInMap, in float TopZ, in float ButtonZ, in GridMap2DData map) =>
        (pos - sensorInMap).Z < TopZ &&
        (pos - sensorInMap).Z > ButtonZ && OccupancyGridMapBuilder.CheckBound2D(pos, map);
    #endregion

    #region 二维带高度占据栅格地图生成
    private static void UpdateHighRateFromPointThreadSafety(in Vector3 point,
                                                            in Vector3 form,
                                                            in Vector3 chassisStep,
                                                            OccupancyHighGrid2DMap map)
    {
        var data = map.Data.OccupancyRate.AsSpan();
        var high = map.High.AsSpan();

        var begin =
            new Vector2i(
                (int)Math.Clamp(point.X / map.Data.GridMapData.Resolution, 0, map.Data.GridMapData.Width - 1),
                (int)Math.Clamp(point.Y / map.Data.GridMapData.Resolution, 0, map.Data.GridMapData.Height - 1));
        var end =
            new Vector2i(
                (int)Math.Clamp(form.X / map.Data.GridMapData.Resolution, 0, map.Data.GridMapData.Width - 1),
                (int)Math.Clamp(form.Y / map.Data.GridMapData.Resolution, 0, map.Data.GridMapData.Height - 1));

        var beginIndex = (int)(begin.x + begin.y * map.Data.GridMapData.Width);

        //TODO: 直线裁剪

        var points = Geometry.BresenhamLine(begin, end);

        var errZ = (form.Z - point.Z) / (points.Count - 1);
        var z = point.Z;
        lock (map)
        {
            for (var i = 1; i < points.Count; i++)
            {
                z += errZ;
                var index = (int)(points[i].x + points[i].y * map.Data.GridMapData.Width);
                if (high[index] is float.MaxValue || z < high[index])
                    Interlocked.Exchange(ref high[index], z);
            }

            if (high[beginIndex] is float.MaxValue || point.Z < high[beginIndex])
                Interlocked.Exchange(ref high[beginIndex], point.Z);
        }
    }

    public static void UpdateHighRateWithPointCloud(Vector3[] points,
                                                    Vector3 sensorInMap,
                                                    Vector3 chassisStep,
                                                    OccupancyHighGrid2DMap map)
    {
        if (!OccupancyGridMapBuilder.CheckBound2D(sensorInMap, map.Data.GridMapData))
            return;
        points.Where(x => OccupancyGridMapBuilder.CheckBound3D(x, sensorInMap, map.TopZ, map.ButtonZ, map.Data.GridMapData)).AsParallel()
            .WithDegreeOfParallelism(Environment.ProcessorCount)
            .ForAll(p =>
                OccupancyGridMapBuilder.UpdateHighRateFromPointThreadSafety(p, sensorInMap, chassisStep, map));
        var data = map.High.AsSpan();
        for (var i = 1; i < map.Data.GridMapData.Width - 1; i++)
            for (var j = 1; j < map.Data.GridMapData.Height - 1; j++)
                map.Data.GridMapData.Data.AsSpan()[(int)(i + j * map.Data.GridMapData.Width)] =
                    // sbyte.Clamp(
                    //     (sbyte)(1.0 /
                    //             (Math.Exp(map.Data.OccupancyRate.AsSpan()[
                    //                 (int)(i + j * map.Data.Width)]) + 1)
                    //             * 100), 0, 100);
                    map.Data.GridMapData.Data.AsSpan()[(int)(i + j * map.Data.GridMapData.Width)] =
                        (sbyte)(Math.Abs(data[(int)(i + (j + 1) * map.Data.GridMapData.Width)] -
                                         data[(int)(i + (j - 1) * map.Data.GridMapData.Width)]) +
                                Math.Abs(data[(int)(i - 1 + j * map.Data.GridMapData.Width)] -
                                         data[(int)(i + 1 + j * map.Data.GridMapData.Width)]) 
                                > map.Data.GridMapData.Resolution * 2
                                ? 100
                                : 0);

    }
    #endregion

    #region 二维占据栅格地图生成
    public static void UpdateRateFromPointCloud(Vector3[] points, Vector3 sensorInMap, OccupancyGrid2DMap map)
    {
        if (!OccupancyGridMapBuilder.CheckBound2D(sensorInMap, map.Data.GridMapData))
            return;
        points.Where(x => OccupancyGridMapBuilder.CheckBound3D(x, sensorInMap, map.TopZ, map.ButtonZ, map.Data.GridMapData)).AsParallel()
            .WithDegreeOfParallelism(Environment.ProcessorCount)
            .ForAll(p => OccupancyGridMapBuilder.UpdateRateFromPointThreadSafety(p, sensorInMap, map.Data));

        for (var i = 0; i < map.Data.GridMapData.Width; i++)
            for (var j = 0; j < map.Data.GridMapData.Width; j++)
                map.Data.GridMapData.Data.AsSpan()[(int)(i + j * map.Data.GridMapData.Width)] =
                    sbyte.Clamp(
                        (sbyte)(1.0 /
                                (Math.Exp(map.Data.OccupancyRate.AsSpan()[
                                    (int)(i + j * map.Data.GridMapData.Width)]) + 1)
                                * 100), 0, 100);
    }


    private static void UpdateRateFromPointThreadSafety(in Vector3 point, in Vector3 form, OGMData map)
    {
        var data = map.OccupancyRate.AsSpan();

        var begin =
            new Vector2i(
                (int)Math.Clamp(point.X / map.GridMapData.Resolution, 0, map.GridMapData.Width - 1),
                (int)Math.Clamp(point.Y / map.GridMapData.Resolution, 0, map.GridMapData.Height - 1));
        var end =
            new Vector2i(
                (int)Math.Clamp(form.X / map.GridMapData.Resolution, 0, map.GridMapData.Width - 1),
                (int)Math.Clamp(form.Y / map.GridMapData.Resolution, 0, map.GridMapData.Height - 1));
        //TODO: 直线裁剪
        var points = Geometry.ThickLine(
            begin, end);
        foreach (var p in points)
            OccupancyGridMapBuilder.AtomicAdd(ref data[(int)(p.x + p.y * map.GridMapData.Width)], map.LossFree);

        OccupancyGridMapBuilder.AtomicAdd(
            ref data[(int)(begin.x + begin.y * map.GridMapData.Width)],
            map.LossOccu - map.LossFree);
    }
    #endregion

}