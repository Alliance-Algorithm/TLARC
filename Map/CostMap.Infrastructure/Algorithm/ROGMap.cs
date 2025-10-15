using System.Buffers;
using System.Collections.Concurrent;
using System.Numerics;
using System.Runtime.CompilerServices;
using g4;
using Kernel.Utils;
using MathNet.Numerics.RootFinding;

namespace CostMap.Infrastructure.Algorithm;

public static class ROGMap
{

    #region  Utils
    internal static Vector3i Index3(in Vector3 point, in Infrastructure.Data.ROGMap rogMap)
    {
        var i_l_x = (int)Math.Round(point.X / rogMap.Resolution);
        var i_l_y = (int)Math.Round(point.Y / rogMap.Resolution);
        var i_l_z = (int)Math.Round((point.Z - rogMap.ButtonZ) / rogMap.Resolution);

        return new(i_l_x, i_l_y, i_l_z);
    }
    internal static Vector2i Index(in Vector3 point, in Infrastructure.Data.ROGMap rogMap)
    {
        var i_l_x = (int)Math.Round(point.X / rogMap.Resolution);
        var i_l_y = (int)Math.Round(point.Y / rogMap.Resolution);

        return new(i_l_x, i_l_y);
    }

    internal static Vector3i Normalize(this Vector3i vec, in Infrastructure.Data.ROGMap rogMap)
    {

        var i_l_x = vec.x;
        var i_l_y = vec.y;

        i_l_x %= rogMap.SizeX;
        i_l_y %= rogMap.SizeY;
        while (i_l_x < 0) i_l_x += rogMap.SizeX;
        while (i_l_y < 0) i_l_y += rogMap.SizeY;

        vec.x = i_l_x;
        vec.y = i_l_y;
        vec.z = Math.Clamp(vec.z, 0, rogMap.SizeZ - 1);

        return vec;
    }
    internal static Vector3i LocalToGlobalNormalize(this Vector3i vec, in Infrastructure.Data.ROGMap rogMap)
    {
        vec.x += rogMap.CenterX - rogMap.s_x_2;
        vec.y += rogMap.CenterY - rogMap.s_y_2;
        return vec.Normalize(rogMap);
    }

    internal static Vector2i LocalToGlobalNormalize(this Vector2i vec, in Infrastructure.Data.ROGMap rogMap)
    {
        vec += rogMap._center;
        vec.x -= rogMap.s_x_2;
        vec.y -= rogMap.s_y_2;
        return vec.Normalize(rogMap);
    }

    internal static Vector2i Normalize(this Vector2i vec, in Infrastructure.Data.ROGMap rogMap)
    {

        var i_l_x = vec.x;
        var i_l_y = vec.y;

        i_l_x %= rogMap.SizeX;
        i_l_y %= rogMap.SizeY;
        if (i_l_x < 0) i_l_x += rogMap.SizeX;
        if (i_l_y < 0) i_l_y += rogMap.SizeY;

        vec.x = i_l_x;
        vec.y = i_l_y;

        return vec;
    }

    #endregion
    private static void UpdateLocalMapOrigin(this Data.ROGMap rogMap, in Vector3 x_k) => rogMap._center = Index(x_k, rogMap);

    private static void ResetMemoryOutsideMap(this Data.ROGMap rogMap, Vector2i c_last)
    {
        var err = rogMap._center - c_last;
        var s_x_2 = rogMap.s_x_2;
        var s_y_2 = rogMap.s_y_2;

        void helper((int x, int y, int sx, int sy) range)
        {
            for (int i = range.x; i < range.sx; i++)
                for (int j = range.y; j < range.sy; j++)
                {
                    var point = new Vector2i(i, j).Normalize(rogMap);
                    var pointIndex = point.x + point.y * rogMap.SizeX;

                    rogMap._gridData[pointIndex] = 0;
                    for (int k = 0; k < rogMap.SizeZ; k++)
                        rogMap._memory[pointIndex + k * rogMap.Size2D] = 0;
                    rogMap.Data[pointIndex] = 0;

                    rogMap._upper[pointIndex] = -1e6f;
                    rogMap._lower[pointIndex] = 1e6f;
                }
        }

        var centerX = err.x > 0 ? rogMap.CenterX - s_x_2 : rogMap.CenterX + s_x_2;
        var centerY = err.y > 0 ? rogMap.CenterY - s_y_2 : rogMap.CenterY + s_y_2;
        var originX = c_last.x - s_x_2;
        var originY = c_last.y - s_y_2;
        var endX = c_last.x + s_x_2 + 1;
        var endY = c_last.y + s_y_2 + 1;

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
                (1, 1) => [ (originX, originY, centerX,      centerY),
                            (originX, centerY, centerX,      endY),
                            (centerX, originY, endX, centerY)],

                (1, -1) => [(originX, originY, centerX,      centerY),
                            (originX, centerY, centerX,      endY),
                            (centerX, centerY, endX,         endY)],

                (-1, -1) => [   (centerX, centerY, endX,        endY),
                                (originX, centerY, centerX,     endY),
                                (centerX, originY, endX,        centerY)],

                (-1, 1) => [(originX, originY, centerX,      centerY),
                            (centerX, centerY, endX,         endY),
                            (centerX, originY, endX,         centerY)],
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

    private static void Raycasting(this Data.ROGMap rogMap, Vector3 x_k, in Vector3[] p, ref Memory<sbyte> c)
    {
        var s_x_2 = (rogMap.s_x_2 - rogMap._inflationDistanceHalf) * rogMap.Resolution;
        var s_y_2 = (rogMap.s_y_2 - rogMap._inflationDistanceHalf) * rogMap.Resolution;
        bool func(Vector3 x) => Math.Abs(x.X) <= s_x_2 && Math.Abs(x.Y) <= s_y_2
                                        && (new Vector3(x.X, x.Y, 0) - new Vector3(x_k.X, x_k.Y, 0)).Length() > rogMap.BlindCircleRadius
                                        && x.Z <= rogMap.TopZ && x.Z >= rogMap.ButtonZ;

        using var cMemoryHandle = c.Pin();

        unsafe
        {

            var cPtr = (sbyte*)cMemoryHandle.Pointer;
            p.AsParallel().Where(func).AsParallel().WithDegreeOfParallelism(Environment.ProcessorCount).ForAll(
                p =>
                {
                    Geometry.Bresenham3D(Index3(p, rogMap), Index3(x_k, rogMap)).ForEach(
                        i =>
                        {
                            i.x += rogMap.CenterX;
                            i.y += rogMap.CenterY;
                            var index = i.Normalize(rogMap);
                            var idx = index.x + index.y * rogMap.SizeX;
                            var x = idx + index.z * rogMap.Size2D;
                            rogMap.UpdateFrameCount[idx] = 0;
                            float curr, min; do curr = rogMap.Memory[x];
                            while (Interlocked.CompareExchange(ref rogMap.Memory[x],
                                Math.Clamp(rogMap.Memory[x] + rogMap._lossMiss, -rogMap._lossMax, rogMap._lossMax), curr) != curr);
                            do
                            {
                                curr = rogMap._lower[idx];
                                min = Math.Min(rogMap._lower[idx], i.z);
                            }
                            while (rogMap._lower[idx] > i.z && Interlocked.CompareExchange(ref rogMap._lower[idx], min, curr) != curr);
                        }
                        );

                    var point = Index3(p, rogMap);
                    point.x += rogMap.CenterX;
                    point.y += rogMap.CenterY;
                    point = point.Normalize(rogMap);
                    var idx = point.x + point.y * rogMap.SizeX;
                    var pointIndex = idx + point.z * rogMap.Size2D;
                    float curr; do curr = rogMap.Memory[pointIndex];
                    while (Interlocked.CompareExchange(ref rogMap.Memory[pointIndex],
                        Math.Clamp(rogMap.Memory[pointIndex] + rogMap._lossHit - rogMap._lossMiss, -rogMap._lossMax, rogMap._lossMax), curr) != curr);
                    rogMap.UpdateFrameCount[idx] = 0;
                }
            );
            BlockParallel.For(rogMap.SizeX, rogMap.SizeY, 0, 0, (x, y) =>
            {
                Vector2i temp;
                temp.x = x;
                temp.y = y;
                temp.LocalToGlobalNormalize(rogMap);
                int index = temp.x + temp.y * rogMap.SizeX;
                var cnt = 0;
                for (int i = 0; i < rogMap.SizeZ; i++)
                {
                    var pointIndex = index + i * rogMap.Size2D;

                    if (rogMap.Memory[pointIndex] > rogMap._lossOccu)
                    {
                        cnt++;
                        float z = i;
                        // float curr, min;
                        // do
                        // {
                        //     curr = rogMap._upper[index];
                        //     min = Math.Max(rogMap._upper[index], z);
                        // }
                        // while (rogMap._upper[index] < z && Interlocked.CompareExchange(ref rogMap._upper[index], min, curr) != curr);
                        // do
                        // {
                        //     curr = rogMap._lower[index];
                        //     min = Math.Min(rogMap._lower[index], z);
                        // }
                        // while (rogMap._lower[index] > z && Interlocked.CompareExchange(ref rogMap._lower[index], min, curr) != curr);
                        rogMap._upper[index] = Math.Max(rogMap._upper[index], z);
                        rogMap._lower[index] = Math.Min(rogMap._lower[index], z);
                    }
                }

                if ((rogMap._gridData[index] & 0x80000000) == 0 && cnt / (rogMap._upper[index] - rogMap._lower[index]) > rogMap._occuDensity && (rogMap._upper[index] - rogMap._lower[index]) > rogMap._highError)
                {
                    rogMap._gridData[index] |= 0x80000000;
                    *(cPtr + index) = 1;
                }
                else if ((rogMap._gridData[index] & 0x80000000) != 0 && (cnt / (rogMap._upper[index] - rogMap._lower[index]) < rogMap._occuDensity || (rogMap._upper[index] - rogMap._lower[index]) < rogMap._highError))
                {
                    rogMap._gridData[index] &= 0x7fffffff;

                    *(cPtr + index) = -1;
                }
            });

            Parallel.For(0, rogMap.UpdateFrameCount.Length, i =>
            {
                if (rogMap.UpdateFrameCount[i] < rogMap.ForgetFrameCount)
                    rogMap.UpdateFrameCount[i]++;
                else
                {
                    if ((rogMap._gridData[i] & 0x80000000) != 0)
                        *(cPtr + i) = -1;
                    rogMap._gridData[i] &= 0x7fffffff;
                    for (int k = 0; k < rogMap.SizeZ; k++)
                        rogMap._memory[i + k * rogMap.Size2D] = 0;
                    rogMap.Data[i] = 0;
                    rogMap._upper[i] = -1e6f;
                    rogMap._lower[i] = 1e6f;
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
            void func(int x, int y)
            {
                var center = new Vector2i(x, y).LocalToGlobalNormalize(rogMap);
                var flag = *(cPtr + center.x + center.y * rogMap.SizeX);
                if (flag == 0)
                    return;
                for (int i = 0; i < rogMap._inflationDistance; i++)
                    for (int j = 0; j < rogMap._inflationDistance; j++)
                    {
                        var cx = i + x - rogMap._inflationDistanceHalf;
                        var cy = j + y - rogMap._inflationDistanceHalf;

                        if (cx < 0 || cy < 0 || cx >= rogMap.SizeX || cy >= rogMap.SizeY) continue;

                        Vector2i c = new Vector2i(cx, cy).LocalToGlobalNormalize(rogMap);
                        var index = c.x + c.y * rogMap.SizeX;
                        int tmp = Unsafe.BitCast<uint, int>(rogMap._gridData[index] & 0x7fffffff);
                        tmp = Math.Clamp(tmp + flag, 0, 0x0fffffff);
                        rogMap._gridData[index] = Unsafe.BitCast<int, uint>(tmp) | (rogMap._gridData[index] & 0x80000000);
                    }
            }
            BlockParallel.For(
                rogMap.SizeX, rogMap.SizeY, rogMap._inflationDistance, rogMap._inflationDistance, func
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
        var d = rogMap._slidingThreshold;

        if ((x - o).Length() > d)
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
    /// <param name="robotPositionInMapCenter">robot position in center tf node</param>
    /// <param name="pointCloudInMapCenter">point list in map center tf node</param>
    public static void MapUpdate(in Data.ROGMap rogMap, in Vector3 robotPositionInMapCenter, in Vector3[] pointCloudInMapCenter)
    {
        var x = new Vector3(robotPositionInMapCenter.X, robotPositionInMapCenter.Y, 0);

        using var memoryOwner = System.Buffers.MemoryPool<sbyte>.Shared.Rent(rogMap.Size2D);
        var memoryHandle = memoryOwner.Memory;
        memoryHandle.Span.Clear();
        rogMap.Raycasting(robotPositionInMapCenter, pointCloudInMapCenter, ref memoryHandle);
        rogMap.IncrementalInflation(memoryHandle);

    }
    /// <summary>
    /// 更新GridMap,将ROGMap 转换为正常布局，一般只会给Visualizer使用
    /// </summary>
    /// <param name="rogMap"></param>
    public static void UpdateGridMap(Data.ROGMap rogMap)
    {
        BlockParallel.For(
            rogMap.SizeX, rogMap.SizeY, 0, 0,
            (x, y) =>
            {
                var c = new Vector2i(x, y).LocalToGlobalNormalize(rogMap);
                var index = c.x + c.y * rogMap.SizeX;
                var step = rogMap.TopZ - rogMap.ButtonZ;
                rogMap.Data[x + y * rogMap.SizeX] = (sbyte)((
                    rogMap._gridData[index] != 0
                        ) ? 100 : (rogMap._lower[index] == 1e6 ? 0 : Math.Clamp(rogMap._lower[index] * rogMap.Resolution / step, 0.25, 1) * 49));
            });
    }
}