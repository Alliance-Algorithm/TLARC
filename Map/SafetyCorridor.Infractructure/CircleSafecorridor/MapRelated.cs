using System.Collections.ObjectModel;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using Kernel.DataInterfaces;
using Kernel.DataInterfaces.Constraints;
using Kernel.DataInterfaces.Navigation;

namespace SafetyCorridor.Infractructure.CircleSafecorridor;

public class Sdf2DRelated : IObstacle
{
    public required IHeader Header { get; init; }

    public bool FindNearestObstacleDistance(Vector2 from, float radius, out float obstacles)
    {
        obstacles = -1;
        if (MapData.IsMoveAble(from, out var dis) || dis is 0)
            return false;
        obstacles = Math.Min(dis, radius);
        return true;
    }
    public required ISdf2D MapData { get; init; }
}