using System.Numerics;
using Kernel.DataInterfaces;
using Kernel.DataInterfaces.Constraints;
using Kernel.DataInterfaces.Navigation;
using Kernel.DataInterfaces.Visualization;

namespace ALPlanner.Infrastructure.SafeCorridorConstruct;

public static class GaussianSample
{

    public class SafeCorridorDecorator(LinkedList<Circle2D> circle2Ds, IHeader header) : ISafeCorridor2DData<Circle2D>
    {
        public IHeader Header => header;

        public int Length => Corridors.Length;

        public Circle2D[] Corridors => [.. circle2Ds];
    }
    static readonly Vector2[] GaussianOffset = [
        new (0, 0),
        new (0.01f,0),
        new (-0.01f,0),
        new (0,0.01f),
        new (0,-0.01f),
        new (0.01f,-0.01f),
        new (-0.01f,0.01f),
        new (0.01f,0.01f),
        new (-0.01f,-0.01f),
        new (0.02f,0),
        new (-0.02f,0),
        new (0,0.02f),
        new (0,-0.02f),
        new (0.02f,-0.02f),
        new (-0.02f,0.02f),
        new (0.02f,0.02f),
        new (-0.2f,-0.2f),
        new (0.01f,0.02f),
        new (-0.02f,0.01f),
        new (0.05f,0.05f),
        new (-0.05f,0.05f),
        new (0.05f,-0.05f),
        new (-0.05f,-0.05f),
        ];
    public static ISafeCorridor2DData<Circle2D> RadiusWithDistance(IPath2D astarPath, IObstacle obstacle)
    {
        LinkedList<Circle2D> corridor = [];
        int count = 0;
        foreach (var path in astarPath.GetPoints())
        {
            if (corridor.Count == 0)
            {
                obstacle.FindNearestObstacleDistance(path, 2, out var dist);
                corridor.AddLast(new Circle2D(dist, path));
                continue;
            }

            if (corridor.Last!.Value.R >= (path - corridor.Last!.Value.Origin).Length())
                continue;

            float dis = 0;
            corridor.AddLast((from offset in GaussianOffset
                              where obstacle.FindNearestObstacleDistance(path + offset, 2, out dis)
                                    && corridor.Last!.Value.R < (path + offset - corridor.Last!.Value.Origin).Length()
                              select new Circle2D(Math.Clamp(dis, ((path + offset - corridor.Last!.Value.Origin).Length() - corridor.Last!.Value.R) * 1.1f, ((path + offset - corridor.Last!.Value.Origin).Length() - corridor.Last!.Value.R) * 1.2f), path + offset))
                              .MaxBy(item => item.R)!);

            // if (++count is 10) break;
        }
        return new SafeCorridorDecorator(corridor, obstacle.Header);
    }
}