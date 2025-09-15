using System.Collections;
using System.Numerics;
using System.Runtime.CompilerServices;
using Kernel.DataInterfaces.Navigation;

namespace ALPlanner.Infrastructure.PathSearcher;

public class AStar(int sizeX, int sizeY, float step, float mapResolution)
{
    readonly float _resolution = mapResolution;
    readonly float _step = step;
    readonly Vector2[] _steps = [new(step, 0), new(-step, 0), new(0, step), new(0, -step),
                                new(step / MathF.Sqrt(2), step / MathF.Sqrt(2)),
                                new(-step / MathF.Sqrt(2), step / MathF.Sqrt(2)),
                                new(-step / MathF.Sqrt(2), -step / MathF.Sqrt(2)),
                                new(step / MathF.Sqrt(2), -step / MathF.Sqrt(2)),];
    readonly PriorityQueue<AStarNode, float> _openList = new();
    readonly float[] _closeMap = new float[sizeX * sizeY];


    internal (int x, int y) Index(in Vector2 point)
    {
        var i_l_x = (int)Math.Round(point.X / _resolution);
        var i_l_y = (int)Math.Round(point.Y / _resolution);

        return new(i_l_x, i_l_y);
    }

    internal (int x, int y) Normalize((int x, int y) vec)
    {

        var i_l_x = vec.x;
        var i_l_y = vec.y;

        i_l_x %= sizeX;
        i_l_y %= sizeY;
        while (i_l_x < 0) i_l_x += sizeX;
        while (i_l_y < 0) i_l_y += sizeY;

        vec.x = i_l_x;
        vec.y = i_l_y;

        return vec;
    }
    /// <summary>
    /// Find path in map tf node
    /// </summary>
    /// <param name="from">from in mapOriginTfNode</param>
    /// <param name="to">to in mapOriginTfNode</param>
    /// <param name="map"></param>
    /// <returns></returns>
    public Path Search(Vector2 from, Vector2 to, ISdf2D map)
    {
        var closeSpan = _closeMap.AsSpan();
        closeSpan.Clear();
        _openList.Clear();

        AStarNode begin = new(0, 0, from, null);
        AStarNode end = new(0, 0, to, null);

        _openList.Enqueue(begin, 0);
        while (_openList.Count > 0)
        {
            var current = _openList.Dequeue();

            if ((current.point - end.point).Length() < _step)
            {
                end.Parent = current.Parent;
                break;
            }
            var (indexX, indexY) = Normalize(Index(current.point));
            var index = indexX + indexY * sizeX;
            if (closeSpan[index] < 0)
                continue;
            closeSpan[index] = -1;

            var points = from p in _steps select p + current.point;
            float f = 0;
            var children = from p in points
                           where !map.IsMoveAble(p, out f)
                           select new AStarNode((end.point - p).Length() - f * 2,
                                                current.G + _step,
                                                p, current);

            foreach (var child in children)
            {
                (indexX, indexY) = Normalize(Index(child.point));
                index = indexX + indexY * sizeX;
                if (closeSpan[index] != 0 && closeSpan[index] < child.G)
                    continue;
                closeSpan[index] = child.G;
                _openList.Enqueue(child, child.F);
            }
        }
        var s = end.ToStack();
        return new Path(map.Header.Identifier, s.Count, s);
    }
}

class AStarNode(in float h, in float g, in Vector2 point, AStarNode? parent)
{
    public readonly Vector2 point = point;
    public readonly float H = h;
    public readonly float G = g;
    public readonly float F = h + g;
    public AStarNode? Parent = parent;
    public Stack<Vector2> ToStack()
    {
        Stack<Vector2> pathPoints = new();
        AStarNode? current = this;

        while (current != null)
        {
            pathPoints.Push(current.point);
            current = current.Parent;
        }
        return pathPoints;
    }
}