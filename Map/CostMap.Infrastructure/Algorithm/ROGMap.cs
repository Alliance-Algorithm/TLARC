using System.Buffers;
using System.Collections.Concurrent;
using System.Numerics;
using System.Runtime.CompilerServices;
using g4;
using Kernel.Utils;

namespace CostMap.Infrastructure.Algorithm;

public static class ROGMap
{

    #region  Utils
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    static Vector2i Index(in Vector3 point, in Infrastructure.Data.ROGMap rogMap)
    {
        var i_l_x = (int)Math.Round(point.X / rogMap.Resolution);
        var i_l_y = (int)Math.Round(point.Y / rogMap.Resolution);

        return new(i_l_x, i_l_y);
    }

    static Vector2i Normalize(this Vector2i vec, in Infrastructure.Data.ROGMap rogMap)
    {

        var i_l_x = vec.x;
        var i_l_y = vec.y;

        var s_x_2 = rogMap.SizeX / 2;
        var s_y_2 = rogMap.SizeX / 2;

        var s_x_3_2 = 3 * rogMap.SizeX / 2;
        var s_y_3_2 = 3 * rogMap.SizeX / 2;

        while (i_l_x < -s_x_2) i_l_x += s_x_3_2;
        while (i_l_y < -s_y_2) i_l_y += s_y_3_2;
        while (i_l_x > s_x_2) i_l_x -= s_x_2;
        while (i_l_y > s_y_2) i_l_y -= s_y_2;

        i_l_x += s_x_2;
        i_l_y += s_y_2;

        vec.x = i_l_x;
        vec.y = i_l_y;

        return vec;
    }

    #endregion
    private static void UpdateLocalMapOrigin(this Data.ROGMap rogMap, in Vector3 x_k) => rogMap._center = Index(x_k, rogMap);

    private static void ResetMemoryOutsideMap(this Data.ROGMap rogMap, Vector2i c_last)
    {
        var err = rogMap._center - c_last;
        var s_x_2 = rogMap.SizeX / 2;
        var s_y_2 = rogMap.SizeX / 2;

        void helper((int x, int y, int sx, int sy) range)
        {
            for (int i = range.x; i < range.sx; i++)
                for (int j = range.y; j < range.sy; j++)
                {
                    var point = new Vector2i(i, j).Normalize(rogMap);
                    var pointIndex = point.x + point.y * rogMap.SizeX;
                    rogMap._gridData[pointIndex] = 0;
                    rogMap.Memory[pointIndex] = 0;
                }
        }

        var centerX = int.Max(c_last.x, rogMap.CenterX) - s_x_2;
        var centerY = int.Max(c_last.y, rogMap.CenterY) - s_y_2;
        var originX = c_last.x - s_x_2;
        var originY = c_last.y - s_x_2;
        var endX = c_last.x + s_x_2 + 1;
        var endY = c_last.y + s_x_2 + 1;

        Vector2i flag;
        if (err.x > s_x_2)
            flag.x = 2;
        else if (err.x > 0)
            flag.x = 1;
        else if (err.x == 0)
            flag.x = 0;
        else if (err.x >= -s_x_2)
            flag.x = -1;
        else flag.x = 2;
        if (err.y > s_y_2)
            flag.y = 2;
        else if (err.y > 0)
            flag.y = 1;
        else if (err.y == 0)
            flag.y = 0;
        else if (err.y >= -s_y_2)
            flag.y = -1;
        else flag.y = 2;

        if (flag.x == 2 || flag.y == 2)
        {
            Array.Clear(rogMap.Memory);
            Array.Clear(rogMap._gridData);
        }
        else if (flag == Vector2i.Zero)
            return;
        else if (flag is { x: -1 or 1, y: -1 or 1 })
        {
            (int x, int y, int sx, int sy)[] values = (flag.x, flag.y) switch
            {
                (1, 1) => [ (originX, originY, centerX,      centerX),
                            (originX, centerY, centerX,      endY),
                            (centerX, originY, endX, centerY)],

                (1, -1) => [(originX, originY, centerX,      centerX),
                            (originX, centerY, centerX,      endY),
                            (centerX, centerY, endX, endY)],

                (-1, -1) => [   (centerX, centerY, endX, endY),
                                (originX, centerY, centerX,      endY),
                                (centerX, originY, endX, centerY)],

                (-1, 1) => [(originX, originY, centerX,      centerX),
                            (centerX, centerY, endX, endY),
                            (centerX, originY, endX, centerY)],
                _ => [],
            };

            values.AsParallel().ForAll(helper);
        }
        else
        {
            switch (flag.x, flag.y)
            {
                case (1, 0):
                    helper((originX, originY, centerX, endY));
                    break;
                case (0, 1):
                    helper((originX, originY, endX, centerY));
                    break;
                case (-1, 0):
                    helper((centerX, originY, endX, endY));
                    break;
                case (0, -1):
                    helper((originX, centerY, endX, endY));
                    break;
                default: throw new Exception("RogMap err: Reset Memory");
            }
        ;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Raycasting(this Data.ROGMap rogMap, Vector3 x_k, in Vector3[] p, ref Memory<sbyte> c)
    {
        var s_x_2 = rogMap.SizeX / 2 * rogMap.Resolution;
        var s_y_2 = rogMap.SizeY / 2 * rogMap.Resolution;
        bool func(Vector3 x) => Math.Abs(x.X) <= s_x_2 && Math.Abs(x.Y) <= s_y_2 && (x - x_k).Length() > rogMap.BlindCircleRadius;
        var para_p = p.AsParallel().Where(func).ToList().AsParallel();

        Parallel.For(0, rogMap.UpdateFrameCount.Length, i =>
        {
            if (rogMap.UpdateFrameCount[i] < rogMap.ForgetFrameCount)
                rogMap.UpdateFrameCount[i]++;
            else
            {
                rogMap._upper[i] = -1e6f;
                rogMap._lower[i] = 1e6f;
            }
        });

        using var hitMemoryOwner = System.Buffers.MemoryPool<int>.Shared.Rent(rogMap.SizeX * rogMap.SizeY);
        using var missMemoryOwner = System.Buffers.MemoryPool<int>.Shared.Rent(rogMap.SizeX * rogMap.SizeY);
        using var hitMemoryHandle = hitMemoryOwner.Memory.Pin();
        using var missMemoryHandle = missMemoryOwner.Memory.Pin();
        using var cMemoryHandle = c.Pin();
        unsafe
        {
            var cPtr = (int*)cMemoryHandle.Pointer;
            var missPtr = (int*)missMemoryHandle.Pointer;
            var hitPtr = (int*)hitMemoryHandle.Pointer;
            para_p.WithDegreeOfParallelism(Environment.ProcessorCount).ForAll(
                p =>
                {
                    Geometry.BresenhamLine(Index(p, rogMap), Index(x_k, rogMap)).ForEach(
                        i =>
                        {
                            var index = i.Normalize(rogMap);
                            var x = index.x + index.y * rogMap.SizeX;
                            Interlocked.Increment(ref *(missPtr + x));
                        }
                        );

                    var point = Index(x_k, rogMap).Normalize(rogMap);
                    var pointIndex = point.x + point.y * rogMap.SizeX;
                    Interlocked.Increment(ref *(hitPtr + pointIndex));
                    float curr, min;
                    do
                    {
                        curr = rogMap._lower[pointIndex];
                        min = Math.Min(rogMap._lower[pointIndex], p.Z);
                    }
                    while (rogMap._lower[pointIndex] > p.Z && Interlocked.CompareExchange(ref rogMap._lower[pointIndex], curr, min) != curr);
                    do
                    {
                        curr = rogMap._upper[pointIndex];
                        min = Math.Max(rogMap._upper[pointIndex], p.Z);
                    }
                    while (rogMap._upper[pointIndex] < p.Z && Interlocked.CompareExchange(ref rogMap._upper[pointIndex], curr, min) != curr);
                }
            );
            BlockParallel.For(rogMap.SizeX, rogMap.SizeY, 0, 0, (x, y) =>
            {
                Vector2i temp;
                temp.x = x;
                temp.y = y;
                temp.Normalize(rogMap);
                int index = temp.x + temp.y * rogMap.SizeX;
                rogMap.Memory[index] = Math.Clamp(
                    rogMap.Memory[index] + *(missPtr + index) * rogMap._lossMiss + *(hitPtr + index) * rogMap._lossHit,
                     -rogMap._lossMax, rogMap._lossMax);

                if ((rogMap._gridData[index] & 0x80) != 0 && rogMap.Memory[index] > rogMap._lossOccu)
                {
                    rogMap._gridData[index] &= 0x7fffffff;
                    *(cPtr + x) = 1;
                }
                else if ((rogMap._gridData[index] | 0x80) == 0 && rogMap.Memory[index] < rogMap._lossFree)
                {
                    rogMap._gridData[index] |= 0x80000000;
                    *(cPtr + x) = -1;
                }
            });

        }

    }

    private static void IncrementalInflation(this Data.ROGMap rogMap, in Memory<sbyte> c)
    {

        unsafe
        {
            using var cMemory = c.Pin();
            var cPtr = (sbyte*)cMemory.Pointer;
            BlockParallel.For(
                rogMap.SizeX, rogMap.SizeY, rogMap._inflationDistance, rogMap._inflationDistance,
                (x, y) =>
                {
                    var flag = *(cPtr + x + y * rogMap.SizeX);
                    for (int i = 0; i < rogMap._inflationDistance; i++)
                        for (int j = 0; j < rogMap._inflationDistance; j++)
                        {
                            if (i == j) continue;
                            Vector2i c;
                            var cx = i + x - rogMap._inflationDistanceHalf;
                            var cy = j + y - rogMap._inflationDistanceHalf;

                            if (cx < 0 || cy < 0 || cx >= rogMap.SizeX || cy >= rogMap.SizeY) continue;
                            c.x = cx;
                            c.y = cy;
                            c.Normalize(rogMap);
                            var index = c.x + c.y * rogMap.SizeX;
                            var ptr = cPtr + index;
                            int tmp = Unsafe.BitCast<uint, int>(rogMap._gridData[index] & 0x7fffffff);
                            tmp = Math.Clamp(tmp + flag, 0, 0x0fffffff);
                            rogMap._gridData[index] = Unsafe.BitCast<int, uint>(tmp) | (rogMap._gridData[index] & 0x80000000);
                        }
                }
            );
        }
    }

    /// <summary>
    /// <para>Slide ROGMap with current frame data</para>
    /// ROGMap Alg.1 At Sec.IV part1
    /// </summary>
    /// <param name="rogMap"></param>
    /// <param name="robotPositionInGlobal">robot position in global tf node</param>
    public static void MapSliding(in Data.ROGMap rogMap, in Vector3 robotPositionInGlobal)
    {
        var x = new Vector3(robotPositionInGlobal.X, robotPositionInGlobal.Y, 0);
        var o = new Vector3(rogMap.CenterX * rogMap.Resolution, rogMap.CenterY * rogMap.Resolution, 0);
        var d = rogMap.SlidingThreshold;

        if ((x - o).Length() < d)
        {
            var old = rogMap._center;
            rogMap.UpdateLocalMapOrigin(x);
            rogMap.ResetMemoryOutsideMap(old);
        }

    }
    /// <summary>
    /// <para>Update ROGMap with current frame data</para>
    /// ROGMap Alg.1 At Sec.IV part1
    /// </summary>
    /// <param name="rogMap"></param>
    /// <param name="robotPositionInGlobal">robot position in global tf node</param>
    /// <param name="pointCloudInMapCenter">point list in map center tf node</param>
    public static void MapUpdate(in Data.ROGMap rogMap, in Vector3 robotPositionInGlobal, in Vector3[] pointCloudInMapCenter)
    {
        var x = new Vector3(robotPositionInGlobal.X, robotPositionInGlobal.Y, 0);

        using var memoryOwner = System.Buffers.MemoryPool<sbyte>.Shared.Rent(rogMap.SizeX * rogMap.SizeY);
        var memoryHandle = memoryOwner.Memory;
        rogMap.Raycasting(robotPositionInGlobal, pointCloudInMapCenter, ref memoryHandle);
        rogMap.IncrementalInflation(memoryHandle);

    }
    /// <summary>
    /// 更新GridMap,将ROGMap 转换为正常布局，一般只会给Visualizer使用
    /// </summary>
    /// <param name="rogMap"></param>
    public static void UpdateGridMap(Data.ROGMap rogMap)
    {
        BlockParallel.For(
            rogMap.SizeX, rogMap.SizeY, rogMap._inflationDistance, rogMap._inflationDistance,
            (x, y) =>
            {
                var c = new Vector2i(x, y).Normalize(rogMap);
                var index = c.x + c.y * rogMap.SizeX;
                rogMap._data[x + y * rogMap.SizeX] = (sbyte)(rogMap._gridData[index] != 0 ? 100 : 0);
            });
    }
}