using System.Collections.ObjectModel;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using Kernel.Contract;
using Kernel.Contract.Constraints;
using Kernel.Contract.Navigation;

namespace SafetyCorridor.Infractructure.CircleSafecorridor;

// public class Sdf2DRelated : IObstacle
// {
//     public bool FindNearestObstacleDistance(Vector2 from, float radius, out float obstacles)
//     {
//         obstacles = -1;
//         if (MapData.IsMoveAble(from, out var dis) || dis is 0)
//             return false;
//         obstacles = Math.Min(dis, radius);
//         return true;
//     }
//     public required IGridMap2D MapData { get; init; }
// }