using System.Numerics;
using System.Runtime.CompilerServices;
using ALPlanner.Infrastructure.SafeCorridorConstruct;
using Kernel.Contract.Constraints;
using Kernel.Contract.Navigation;
using Kernel.Contract.Visualization;
using MathNet.Numerics.Financial;

namespace ALPlanner.Infrastructure.SafeCorridorConstruct;

public static class IncrementalRectangle
{

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    static void Incremental(
        in IObstacle obs,
        ref byte flag,
        ref float MinX,
        ref float MinY,
        ref float MaxX,
        ref float MaxY
    )
    {
        const float step = 0.2f;

        float
        minEplison,
        tmpX,
        tmpY;

        if (MaxX - MinX > 3f)
            flag &= 0b1010;
        if (MaxY - MinY > 3f)
            flag &= 0b0101;

        if ((flag | 0b0001) != 0) // down
        {
            minEplison = 1e9f;
            tmpX = MinX;
            tmpY = MinY;
            while (true)
            {
                var tag = obs.FindNearestObstacleDistance(new Vector2(tmpX, tmpY), step, out var dis);

                minEplison = Math.Min(minEplison, dis);

                if (tag is false) break;

                tmpY += dis;

                if (tmpY > MaxY) break;
            }

            if (minEplison < step) flag &= 0b1110;
            if (minEplison > 0f) MinX -= minEplison;
        }

        if ((flag | 0b0010) != 0) // right
        {
            minEplison = 1e9f;

            tmpX = MinX;
            tmpY = MinY;
            while (true)
            {
                var tag = obs.FindNearestObstacleDistance(new Vector2(tmpX, tmpY), step, out var dis);

                minEplison = Math.Min(minEplison, dis);

                if (tag is false) break;

                tmpX += dis;

                if (tmpX > MaxX) break;
            }

            if (minEplison < step) flag &= 0b1101;
            if (minEplison > 0f) MinY -= minEplison;
        }

        if ((flag | 0b0100) != 0) // up
        {
            minEplison = 1e9f;
            tmpX = MaxX;
            tmpY = MinY;
            while (true)
            {
                var tag = obs.FindNearestObstacleDistance(new Vector2(tmpX, tmpY), step, out var dis);

                minEplison = Math.Min(minEplison, dis);

                if (tag is false) break;

                tmpY += dis;

                if (tmpY > MaxY) break;
            }

            if (minEplison < step) flag &= 0b1011;
            if (minEplison > 0f) MaxX += minEplison;
        }
        if ((flag | 0b1000) != 0) // right
        {
            minEplison = 1e9f;

            tmpX = MinX;
            tmpY = MaxY;
            while (true)
            {
                var tag = obs.FindNearestObstacleDistance(new Vector2(tmpX, tmpY), step, out var dis);

                minEplison = Math.Min(minEplison, dis);

                if (tag is false) break;

                tmpX += dis;

                if (tmpX > MaxX) break;
            }

            if (minEplison < step) flag &= 0b0111;
            if (minEplison > 0f) MaxY += minEplison;
        }

    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    static bool CheckInside(AABB2D aabb, Vector2 point) => point.X < aabb.MaxX && point.Y < aabb.MaxY && point.X > aabb.MinX && point.Y > aabb.MinY;

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    static void Update(in Vector2 p, in IObstacle obstacle, ref LinkedList<AABB2D> corridor)
    {
        var flag = (byte)0b1111;
        float MinX = p.X;
        float MinY = p.Y;
        float MaxX = p.X;
        float MaxY = p.Y;
        while (flag != 0b0000)
            Incremental(
                in obstacle,
                ref flag,
                ref MinX,
                ref MinY,
                ref MaxX,
                ref MaxY);

        corridor.AddLast(new AABB2D{MinX = MinX, MinY = MinY, MaxX = MaxX, MaxY = MaxY});
    }
    public static SafeCorridor2DData<AABB2D> AABBGenerate(Path2D path, IObstacle obstacle)
    {


        LinkedList<AABB2D> corridor = [];

        Update(path.Points[0], obstacle, ref corridor);
        Vector2 last = path.Points[0];
        foreach (var p in path.Points[1 ..])
        {
            while (!CheckInside(corridor.Last!.Value, p))
            {
                var l = (last - p).Length();
                obstacle.FindNearestObstacleDistance(last, l, out var dis);
                if (dis <= 0) break;
                last += (p - last) / l * dis;
                Update(last, obstacle, ref corridor);
            }
            last = p;
            continue;

        }

        return new SafeCorridor2DData<AABB2D>{ Corridors =[.. corridor], Header = path.Header};
    }
}