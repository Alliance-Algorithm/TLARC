using g4;

namespace CostMap.Infrastructure.Algorithm;

internal static class Geometry
{
    /// <summary>
    /// 使用 Bresenham 算法计算两点之间直线经过的所有栅格坐标
    /// </summary>
    /// <param name="from">起点坐标</param>
    /// <param name="to">终点坐标</param>
    /// <returns>直线路径上的所有栅格点</returns>
    private static List<Vector2i> BresenhamLine(Vector2i from, Vector2i to)
    {
        var points = new List<Vector2i>();

        var dx  = Math.Abs(to.x - from.x);
        var dy  = Math.Abs(to.y - from.y);
        var sx  = from.x < to.x ? 1 : -1;
        var sy  = from.y < to.y ? 1 : -1;
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

    /// <summary>
    /// 带厚度参数的直线栅格采样（适用于画线等场景）
    /// </summary>
    internal static IEnumerable<Vector2i> ThickLine(Vector2i from, Vector2i to, int thickness = 1)
    {
        if (thickness <= 1)
            return Geometry.BresenhamLine(from, to);

        var mainLine = Geometry.BresenhamLine(from, to);

        // 计算垂直方向向量
        var dx     = to.x - from.x;
        var dy     = to.y - from.y;
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
        var perpY = dx  / length;

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
}