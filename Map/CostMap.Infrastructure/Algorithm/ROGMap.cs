using System.Buffers;
using System.Collections.Concurrent;
using System.Numerics;
using System.Runtime.CompilerServices;
using CostMap.Infrastructure.Data;
using g4;
using Kernel.Contract;
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
        if (i_l_x < -rogMap.s_x_2)      i_l_x += rogMap.s_x_2 * 3;
        else if(i_l_x > rogMap.s_x_2)   i_l_x -= rogMap.s_x_2;
        else                            i_l_x += rogMap.s_x_2;
        if (i_l_y < -rogMap.s_y_2)      i_l_y += rogMap.s_y_2 * 3;
        else if(i_l_y > rogMap.s_y_2)   i_l_y -= rogMap.s_y_2;
        else                            i_l_y += rogMap.s_y_2;

        vec.x = i_l_x;
        vec.y = i_l_y;
        vec.z = Math.Clamp(vec.z, 0, rogMap.SizeZ - 1);

        return vec;
    }
    internal static Vector3i LocalToGlobalNormalize(this Vector3i vec, in Infrastructure.Data.ROGMap rogMap)
    {
        vec.x += rogMap._center.x - rogMap.s_x_2;
        vec.y += rogMap._center.y - rogMap.s_y_2;
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
        if (i_l_x < -rogMap.s_x_2)      i_l_x += rogMap.s_x_2 * 3;
        else if(i_l_x > rogMap.s_x_2)   i_l_x -= rogMap.s_x_2;
        else                            i_l_x += rogMap.s_x_2;
        if (i_l_y < -rogMap.s_y_2)      i_l_y += rogMap.s_y_2 * 3;
        else if(i_l_y > rogMap.s_y_2)   i_l_y -= rogMap.s_y_2;
        else                            i_l_y += rogMap.s_y_2;

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

                    rogMap._gridData[pointIndex] = Unsafe.BitCast<int,ROGMapCell>(0);
                    for (int k = 0; k < rogMap.SizeZ; k++)
                        rogMap._memory[pointIndex + k * rogMap.Size2D] = 0;
                    rogMap.Data[pointIndex] = 0;

                    rogMap._upper[pointIndex] = -1e6f;
                    rogMap._lower[pointIndex] = 1e6f;
                }
        }

        var centerX = err.x > 0 ? rogMap._center.x - s_x_2 : rogMap._center.x + s_x_2;
        var centerY = err.y > 0 ? rogMap._center.y - s_y_2 : rogMap._center.y + s_y_2;
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
            Array.Clear(rogMap._memory);
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

    private static void Raycasting(this Data.ROGMap rogMap, Vector3 x_k, Vector3[] points, ref Memory<sbyte> c, in int size)
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
            Parallel.For(0, size, index => {
                if(!func(points[index])) return;
                var p = points[index];
                Geometry.Bresenham3D(Index3(p, rogMap), Index3(x_k, rogMap)).ForEach(
                    i =>
                    {
                        i.x += rogMap._center.x;
                        i.y += rogMap._center.y;
                        var index = i.Normalize(rogMap);
                        var idx = index.x + index.y * rogMap.SizeX;
                        var x = idx + index.z * rogMap.Size2D;
                        rogMap.UpdateFrameCount[x] = 0;
                        float curr; do curr = rogMap._memory[x];
                        while (Interlocked.CompareExchange(ref rogMap._memory[x],
                            Math.Clamp(rogMap._memory[x] + rogMap._lossMiss, -rogMap._lossMax, rogMap._lossMax), curr) != curr);
                        
                    }
                    );

                var point = Index3(p, rogMap);
                point.x += rogMap._center.x;
                point.y += rogMap._center.y;
                point = point.Normalize(rogMap);
                var idx = point.x + point.y * rogMap.SizeX;
                var pointIndex = idx + point.z * rogMap.Size2D;
                float curr; do curr = rogMap._memory[pointIndex];
                while (Interlocked.CompareExchange(ref rogMap._memory[pointIndex],
                    Math.Clamp(rogMap._memory[pointIndex] + rogMap._lossHit - rogMap._lossMiss, -rogMap._lossMax, rogMap._lossMax), curr) != curr);
                rogMap.UpdateFrameCount[pointIndex] = 0;
            
            });
            BlockParallel.For(rogMap.SizeX, rogMap.SizeY, 0, 0, (x, y) =>
            {
                Vector2i temp;
                temp.x = x;
                temp.y = y;
                temp.LocalToGlobalNormalize(rogMap);
                int index = temp.x + temp.y * rogMap.SizeX;
                var cnt = 0;
                rogMap._upper[index] = -1e6f;
                rogMap._lower[index] =  1e6f;
                for (int i = 0; i < rogMap.SizeZ; i++)
                {
                    var z = i;
                    var pointIndex = index + i * rogMap.Size2D;

                    if (rogMap._memory[pointIndex] >= rogMap._lossFree && rogMap._memory[pointIndex] <= rogMap._lossOccu)
                        continue;

                    if (rogMap._memory[pointIndex] < rogMap._lossFree)
                    {
                        rogMap._lower[index] = Math.Min(rogMap._lower[index], z);
                        continue;
                    }
                    
                    cnt++;
                    rogMap._upper[index] = Math.Max(rogMap._upper[index], z);
                    rogMap._lower[index] = Math.Min(rogMap._lower[index], z);
                }

                if (    !rogMap._gridData[index].OccupyState && 
                        cnt / (rogMap._upper[index] - rogMap._lower[index]) > rogMap._occuDensity && 
                        (rogMap._upper[index] - rogMap._lower[index]) > rogMap._highError)
                {
                    rogMap._gridData[index].OccupyState = true;
                    *(cPtr + index) = 1;
                }
                else if (cnt / (rogMap._upper[index] - rogMap._lower[index]) <= rogMap._occuDensity ||
                         (rogMap._upper[index] - rogMap._lower[index]) <= rogMap._highError)
                {
                    if(rogMap._gridData[index].OccupyState)
                        *(cPtr + index) = -1;
                    if(rogMap._lower[index] != 1e6f)
                        rogMap._gridData[index].OccupyState = ROGMapCell.StateEnum.Free;
                    else
                        rogMap._gridData[index].OccupyState = ROGMapCell.StateEnum.Unknow;
                }
            });

            Parallel.For(0, rogMap.UpdateFrameCount.Length, i =>
            {
                if (rogMap.UpdateFrameCount[i] < rogMap.ForgetFrameCount)
                    rogMap.UpdateFrameCount[i]++;
                else
                    rogMap._memory[i] = 0;
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
                        var ci = i - rogMap._inflationDistanceHalf;
                        var cj = j - rogMap._inflationDistanceHalf;
                        var cx = i + x - rogMap._inflationDistanceHalf;
                        var cy = j + y - rogMap._inflationDistanceHalf;

                        var value = Math.Sqrt(ci * ci + cj * cj);
                        if (cx < 0 || cy < 0 || cx >= rogMap.SizeX || cy >= rogMap.SizeY || value > rogMap._inflationDistanceHalf) continue;
                        value = value / rogMap._inflationDistanceHalf * 100;
                        Vector2i c = new Vector2i(cx, cy).LocalToGlobalNormalize(rogMap);
                        var index = c.x + c.y * rogMap.SizeX;
                        rogMap._gridData[index].OccupyCount += flag;
                        ref var grid = ref rogMap._gridData[index];
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
        var o = new Vector3(rogMap._center.x * rogMap.Resolution, rogMap._center.y * rogMap.Resolution, 0);
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
    public static void MapUpdate(
        in Data.ROGMap rogMap, 
        in Vector3 robotPositionInMapCenter, 
        in Vector3[] pointCloudInMapCenter,
        in int size)
    {
        using var memoryOwner = System.Buffers.MemoryPool<sbyte>.Shared.Rent(rogMap.Size2D);
        var memoryHandle = memoryOwner.Memory;
        memoryHandle.Span.Clear();
        rogMap.Raycasting(robotPositionInMapCenter, pointCloudInMapCenter, ref memoryHandle,size);
        rogMap.IncrementalInflation(memoryHandle);
    }
    /// <summary>
    /// 更新GridMap,将ROGMap 转换为正常布局，一般只会给Visualizer使用
    /// </summary>
    /// <param name="rogMap"></param>
    public static void UpdateGridMap(Data.ROGMap rogMap)
    {
        void process(int x,int y)
        {
            var c = new Vector2i(x, y).LocalToGlobalNormalize(rogMap);
            var index = c.x + c.y * rogMap.SizeX;
            var step = rogMap.TopZ - rogMap.ButtonZ;
            rogMap.Data[x + y * rogMap.SizeX] = 
                (sbyte)((rogMap._gridData[index].OccupyCount > 0) 
                ? 100 
                : ( rogMap._gridData[index].OccupyState == ROGMapCell.StateEnum.Unknow 
                    ? -1 
                    : Math.Clamp(rogMap._lower[index] * rogMap.Resolution / step, -0.25, 1) * 15 + 20));
        }

        BlockParallel.For(rogMap.SizeX, rogMap.SizeY, 0, 0, process);
    }
}