using System.Numerics;
using Kernel.Contract;
using Kernel.Contract.Constraints;
using Kernel.Contract.Navigation;
using Kernel.Contract.Visualization;

namespace ALPlanner.Infrastructure.SafeCorridorConstruct;

public static class GaussianSample
{

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
    public static SafeCorridor2DData<Circle> RadiusWithDistance<MapT>(Path2D astarPath,MapT obstacle) where MapT : IObstacle
    {
        LinkedList<Circle> corridor = [];
        foreach (var path in astarPath.Points)
        {
            if (corridor.Count == 0)
            {
                obstacle.SearchNearest(path, 2, out var dist);
                corridor.AddLast(new Circle(){R = dist,Origin = path});
                continue;
            }

            if (corridor.Last!.Value.R >= (path - corridor.Last!.Value.Origin).Length())
                continue;

            float dis = 0;
            corridor.AddLast((from offset in GaussianOffset
                              where obstacle.SearchNearest(path + offset, 2, out dis)
                                    && corridor.Last!.Value.R < (path + offset - corridor.Last!.Value.Origin).Length()
                              select new Circle{
                                R = Math.Clamp( dis, 
                                                ((path + offset - corridor.Last!.Value.Origin).Length() - corridor.Last!.Value.R) * 1.1f, 
                                                ((path + offset - corridor.Last!.Value.Origin).Length() - corridor.Last!.Value.R) * 1.2f), 
                                Origin = path + offset})
                              .MaxBy(item => item.R)!);

            // if (++count is 10) break;
        }
        return new SafeCorridor2DData<Circle>{ Corridors = [.. corridor],Header = astarPath.Header};
    }

}