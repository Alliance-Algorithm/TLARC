using MathNet.Numerics.LinearAlgebra;
using GridPoint = System.Numerics.Vector3;

namespace CostMap.Infrastructure.Algorithm;

public class Kriging(float nugget, float sill, float range, List<GridPoint> knownPoints, int maxNeighbors = 16)
{
    private readonly float _nugget = nugget;
    private readonly float _sill = sill;
    private readonly float _range = range;
    private readonly int _maxNeighbors = maxNeighbors;
    private readonly KdTree _kdTree = new(knownPoints);


    public (float height, float variance) Interpolate(float x, float y)
    {
        // 1. 使用空间索引查找最近邻点
        var neighbors = _kdTree.FindNearestNeighbors(x, y, _maxNeighbors);

        if (neighbors.Count < 3)
            return (float.MaxValue, float.NaN); // 邻域点不足

        // 2. 构建协方差矩阵
        var k = BuildCovarianceMatrix(neighbors);

        // 3. 添加无偏约束（单位行/列）
        var a = AddUnbiasedConstraint(k);

        // 4. 构建右端向量
        var b = BuildRightHandVector(neighbors, x, y);

        // 5. Cholesky分解求解
        var weights = SolveWithCholesky(a, b);

        // 6. 计算预测高度和方差
        return CalculateHeightAndVariance(neighbors, weights, x, y);
    }

    private Matrix<float> BuildCovarianceMatrix(List<GridPoint> points)
    {
        var n = points.Count;
        var k = Matrix<float>.Build.Dense(n, n);

        for (var i = 0; i < n; i++)
            for (var j = i; j < n; j++)
            {
                var distance = Distance(points[i], points[j]);
                var cov = Covariance(distance);
                k[i, j] = cov;
                if (i != j) k[j, i] = cov; // 对称矩阵
            }

        return k;
    }

    private static Matrix<float> AddUnbiasedConstraint(Matrix<float> k)
    {
        var n = k.RowCount;
        var a = Matrix<float>.Build.Dense(n + 1, n + 1);

        // 复制原矩阵
        a.SetSubMatrix(0, 0, k);

        // 添加单位行/列
        for (var i = 0; i < n; i++)
        {
            a[i, n] = 1.0f;
            a[n, i] = 1.0f;
        }

        a[n, n] = 0.0f; // 右下角元素

        return a;
    }

    private Vector<float> BuildRightHandVector(
        List<GridPoint> points,
        float x,
        float y)
    {
        var n = points.Count;
        var b = Vector<float>.Build.Dense(n + 1);

        // 未知点到已知点的协方差
        for (var i = 0; i < n; i++)
        {
            var distance = Distance(x, y, points[i].X, points[i].Y);
            b[i] = Covariance(distance);
        }

        b[n] = 1.0f; // 无偏约束

        return b;
    }

    private Vector<float> SolveWithCholesky(
        Matrix<float> a,
        Vector<float> b)
    {
        try
        {
            // Cholesky分解
            var chol = a.Cholesky();
            if (chol == null) throw new Exception("Matrix is not positive definite");

            // 求解方程组
            return chol.Solve(b);
        }
        catch (Exception)
        {
            // 回退到LU分解
            var lu = a.LU();
            return lu.Solve(b);
        }
    }

    private (float height, float variance) CalculateHeightAndVariance(
        List<GridPoint> points,
        Vector<float> weights,
        float x,
        float y)
    {
        var n = points.Count;
        var height = 0.0f;

        // 计算预测高度
        for (var i = 0; i < n; i++)
            height += weights[i] * points[i].Z;

        // 计算预测方差
        var c0 = Covariance(0); // 点自身的协方差
        var variance = c0;

        for (var i = 0; i < n; i++)
        {
            var distance = Distance(x, y, points[i].X, points[i].Y);
            var cov = Covariance(distance);
            variance -= weights[i] * cov;
        }

        // 减去拉格朗日乘数
        variance -= weights[n];

        return (height, Math.Max(0f, variance));
    }

    private float Covariance(float distance)
    {
        // 指数协方差函数
        if (distance == 0)
            return _nugget + _sill;

        return _sill * float.Exp(-distance / _range);
    }

    private float Distance(GridPoint a, GridPoint b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return float.Sqrt(dx * dx + dy * dy);
    }

    private float Distance(float x1, float y1, float x2, float y2)
    {
        var dx = x1 - x2;
        var dy = y1 - y2;
        return float.Sqrt(dx * dx + dy * dy);
    }
}

// KD树空间索引实现
public class KdTree
{
    private readonly Node? _root;
    // private readonly List<GridPoint> _points;

    public KdTree(List<GridPoint> points) =>
        // _points = points;
        _root = BuildTree(points, 0);

    public List<GridPoint> FindNearestNeighbors(float x, float y, int k)
    {
        var nearest = new List<GridPoint>(k);
        var target = new GridPoint(x, y, 0);
        FindNearest(_root, target, k, 0, nearest);
        return nearest;
    }

    private Node? BuildTree(List<GridPoint> points, int depth)
    {
        if (points.Count == 0)
            return null;

        var axis = depth % 2;
        points.Sort((a, b) => axis == 0 ? a.X.CompareTo(b.X) : a.Y.CompareTo(b.Y));

        var mid = points.Count / 2;
        var node = new Node(points[mid]);

        node.Left = BuildTree(points.GetRange(0, mid), depth + 1);
        node.Right = BuildTree(points.GetRange(mid + 1, points.Count - mid - 1), depth + 1);

        return node;
    }

    private void FindNearest(Node? node, GridPoint target, int k, int depth, List<GridPoint> nearest)
    {
        if (node is null) return;

        var axis = depth % 2;
        var nodeValue = axis == 0 ? node.Point.X : node.Point.Y;
        var targetValue = axis == 0 ? target.X : target.Y;

        // 确定搜索路径
        var nearChild = targetValue < nodeValue ? node.Left : node.Right;
        var farChild = targetValue < nodeValue ? node.Right : node.Left;

        // 递归搜索最近子树
        FindNearest(nearChild, target, k, depth + 1, nearest);

        // 添加当前点
        AddToNearest(node.Point, target, k, nearest);

        // 检查远子树是否可能有更近的点
        if (ShouldSearchFarChild(node, target, k, depth, nearest))
            FindNearest(farChild, target, k, depth + 1, nearest);
    }

    private void AddToNearest(GridPoint point, GridPoint target, int k, List<GridPoint> nearest)
    {
        var distance = Distance(point, target);

        // 添加到最近邻列表
        if (nearest.Count < k)
        {
            nearest.Add(point);
            nearest.Sort((a, b) =>
                Distance(a, target).CompareTo(Distance(b, target)));
        }
        else if (distance < Distance(nearest.Last(), target))
        {
            nearest.RemoveAt(nearest.Count - 1);
            nearest.Add(point);
            nearest.Sort((a, b) =>
                Distance(a, target).CompareTo(Distance(b, target)));
        }
    }

    private bool ShouldSearchFarChild(Node node,
                                      GridPoint target,
                                      int k,
                                      int depth,
                                      List<GridPoint> nearest)
    {
        if (nearest.Count < k) return true;

        var axis = depth % 2;
        var nodeValue = axis == 0 ? node.Point.X : node.Point.Y;
        var targetValue = axis == 0 ? target.X : target.Y;
        var distanceToPlane = Math.Abs(nodeValue - targetValue);

        // 如果到分割平面的距离小于当前最大距离，则需要搜索
        return distanceToPlane < Distance(nearest.Last(), target);
    }

    private double Distance(GridPoint a, GridPoint b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    private class Node(GridPoint point)
    {
        public GridPoint Point { get; } = point;
        public Node? Left { get; set; }
        public Node? Right { get; set; }
    }
}
//
// // 使用示例
// public class Program
// {
//     public static void Main()
//     {
//         // 1. 创建已知点数据
//         var knownPoints = new List<GridPoint>
//         {
//             new(1.0, 2.0, 10.5),
//             new(1.5, 2.5, 11.2),
//             new(3.0, 1.0, 9.8)
//             // 添加更多点...
//         };
//
//         // 2. 配置克里金参数
//         var nugget = 0.1; // 块金效应
//         var sill   = 1.0; // 基台值
//         var range  = 10.0; // 变程
//
//         // 3. 创建插值器并构建模型
//         var interpolator = new Kriging(nugget, sill, range);
//         interpolator.BuildModel(knownPoints);
//
//         // 4. 对未知点进行插值
//         var unknownX = 2.0;
//         var unknownY = 1.5;
//
//         var result = interpolator.Interpolate(unknownX, unknownY);
//
//         Console.WriteLine($"预测高度: {result.height:F2}, 方差: {result.variance:F4}");
//     }
// }