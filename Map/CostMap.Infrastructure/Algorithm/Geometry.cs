using System.Collections;
using System.Numerics;
using g4;
using Kernel.Contract;
using Kernel.Contract.Navigation;

namespace CostMap.Infrastructure.Algorithm;

internal static class Geometry
{


    /// <summary>
    /// 使用 Bresenham 算法计算两点之间直线经过的所有栅格坐标
    /// </summary>
    /// <param name="from">起点坐标</param>
    /// <param name="to">终点坐标</param>
    /// <returns>直线路径上的所有栅格点</returns>
    public static List<Vector2i> BresenhamLine(Vector2i from, Vector2i to)
    {
        var points = new List<Vector2i>();

        var dx = Math.Abs(to.x - from.x);
        var dy = Math.Abs(to.y - from.y);
        var sx = from.x < to.x ? 1 : -1;
        var sy = from.y < to.y ? 1 : -1;

        var err = dx - dy;

        var x = from.x;
        var y = from.y;

        while (true)
        {
            points.Add(new Vector2i(x, y));

            if (x == to.x && y == to.y)
                break;

            var e2 = 2 * err;

            if (e2 > -dy)
            {
                err -= dy;
                x += sx;
            }

            if (e2 >= dx)
                break;

            err += dx;
            y += sy;
        }

        return points;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="points">输出点集</param>
    /// <param name="center"></param>
    /// <param name="radius"></param>
    private static void Circle(out List<Vector2i> points, Vector2i center, int radius)
    {
        points = [];
        for (var x = -radius; x <= radius; x++)
            for (var y = -radius; y <= radius; y++)
                if (x * x + y * y <= radius * radius)
                    points.Add(new Vector2i(center.x + x, center.y + y));
    }

    public static List<Vector3i> Bresenham3D(Vector3i from, Vector3i to)
    {
        var voxels = new List<Vector3i>();


        var dx = Math.Abs(to.x - from.x);
        var dy = Math.Abs(to.y - from.y);
        var dz = Math.Abs(to.z - from.z);

        var sx = from.x < to.x ? 1 : -1;
        var sy = from.y < to.y ? 1 : -1;
        var sz = from.z < to.z ? 1 : -1;

        var x0 = from.x;
        var y0 = from.y;
        var z0 = from.z;

        // 确定主导方向
        if (dx > dy)
        {
            // X 主导
            var err1 = 2 * dy - dx;
            var err2 = 2 * dz - dx;

            for (var i = 0; i <= dx; i++)
            {
                voxels.Add(new Vector3i(x0, y0, z0));

                if (err1 > 0)
                {
                    y0 += sy;
                    err1 -= 2 * dx;
                }

                if (err2 > 0)
                {
                    z0 += sz;
                    err2 -= 2 * dx;
                }

                err1 += 2 * dy;
                err2 += 2 * dz;
                x0 += sx;
            }
        }
        else if (dy >= dx)
        {
            // Y 主导
            var err1 = 2 * dz - dy;
            var err2 = 2 * dx - dy;

            for (var i = 0; i <= dy; i++)
            {
                voxels.Add(new Vector3i(x0, y0, z0));

                if (err1 > 0)
                {
                    z0 += sz;
                    err1 -= 2 * dy;
                }

                if (err2 > 0)
                {
                    x0 += sx;
                    err2 -= 2 * dy;
                }

                err1 += 2 * dz;
                err2 += 2 * dx;
                y0 += sy;
            }
        }

        return voxels;
    }

    /// <summary>
    /// 带厚度参数的直线栅格采样（适用于画线等场景）
    /// </summary>
    internal static IEnumerable<Vector2i> ThickLine(Vector2i from, Vector2i to, int thickness = 1)
    {
        if (thickness <= 1)
            return Geometry.BresenhamLine(from, to);

        var mainLine = Geometry.BresenhamLine(from, to);

        // 计算垂直方向向量
        var dx = to.x - from.x;
        var dy = to.y - from.y;
        var length = MathF.Sqrt(dx * dx + dy * dy);

        if (length < float.Epsilon)
        {
            // 起点终点重合，画一个点
            Geometry.Circle(out var circle, from, thickness / 2);
            return circle;
        }

        var points = new HashSet<Vector2i>();

        // 计算垂直方向的单位向量
        var perpX = -dy / length;
        var perpY = dx / length;

        // 为路径上的每个点添加厚度
        foreach (var point in mainLine)
            for (var i = -thickness / 2; i <= thickness / 2; i++)
            {
                var offsetX = (int)(i * perpX);
                var offsetY = (int)(i * perpY);

                points.Add(new Vector2i(point.x + offsetX, point.y + offsetY));
            }

        return points;
    }


    private static byte EncodeCohenSutherland(in Vector2i point,
                                              in Vector2i leftTop,
                                              in Vector2i rightBottom)
    {
        byte result = 0;
        if (point.x >= leftTop.x)
            result |= 0b0001;
        else if (point.x <= rightBottom.x)
            result |= 0b0010;
        if (point.y <= rightBottom.y)
            result |= 0b0100;
        else if (point.y >= leftTop.y)
            result |= 0b1000;
        return result;
    }

    internal static bool CohenSutherland(ref Vector2i from,
                                         ref Vector2i to,
                                         in Vector2i leftTop,
                                         in Vector2i rightBottom,
                                         out bool toState
    )
    {
        var p1 = Geometry.EncodeCohenSutherland(from, leftTop, rightBottom);
        var p2 = Geometry.EncodeCohenSutherland(to, leftTop, rightBottom);
        toState = p2 == 0;
        if ((p1 | p2) == 0)
            return true;
        if ((p1 & p2) == 0)
            return false;

        var mid = (from + to) / 2;

        while (p1 != p2)
        {
            var pt = Geometry.EncodeCohenSutherland(mid, leftTop, rightBottom);
            if (pt == p1)
            {
                from = mid;
                mid = (from + to) / 2;
            }
            else if (pt == p2)
            {
                to = mid;
                mid = (from + to) / 2;
            }

            if (pt == 0)
                if (mid.x == leftTop.x || mid.y == leftTop.y ||
                    mid.x == rightBottom.x || mid.y == rightBottom.y)
                    if (p1 != 0)
                    {
                        from = mid;
                        mid = (from + to) / 2;
                        p1 = 0;
                    }
                    else if (p2 != 0)

                    {
                        to = mid;
                        mid = (from + to) / 2;
                        p2 = 0;
                    }

            if (p1 != 0)
                mid = (from + mid) / 2;
            else if (p2 != 0)
                mid = (to + mid) / 2;
        }

        return true;
    }

    internal enum DilateKernelType
    {
        Euclidean,
        Manhattan
    }

    internal enum DilateForeground
    {
        Max,
        Min
    }

    // internal static void Dilate(GridMap2DData     map,
    //                             DilateKernelType   type,
    //                             DilateForeground   foreground,
    //                             int                lenghtNotAllow,
    //                             int                lenghtPunish,
    //                             int                mapSplitLine,
    //                             out GridMap2DData outMap)
    // {
    //     GridMap2DDataInner inner = new();
    //     inner.Header = map.Header;
    //     inner.Origin = map.Origin;
    //     inner.Width = map.Width;
    //     inner.Height = map.Height;
    //     inner.RotationRad = map.RotationRad;
    //     inner.RotationMatrix = map.RotationMatrix;
    //     inner.Resolution = map.Resolution;
    //     var data = new sbyte[map.Data.Length];
    //
    //
    //     if (lenghtPunish < lenghtNotAllow)
    //         throw new Exception("禁止通行的距离应该小于惩罚距离");
    // }
}