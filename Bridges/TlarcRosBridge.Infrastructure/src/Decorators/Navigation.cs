using g4;
using Rcl;
using Kernel.DataInterfaces.Navigation;
using TlarcRosBridge.Infrastructure.Messages.Nav;

namespace TlarcRosBridge.Infrastructure.Decorators;

public static class Navigation
{
    public static class GridMap
    {
        private class GridMap2DData : IGridMap2DData
        {
            public required Vector2d Origin { get; init; }
            public required Vector2i Size { get; init; }
            public required double RotationRad { get; init; }
            public required Matrix2d RotationMatrix { get; init; }
            public required double Resolution { get; init; }
            public required sbyte[] Data { get; init; }
        }

        public static IGridMap2DData Convert(ref OccupancyGrid.Priv map)
        {
            var rad = double.Asin(2 * (map.Info.Origin.Orientation.W * map.Info.Origin.Orientation.Y -
                                       map.Info.Origin.Orientation.X * map.Info.Origin.Orientation.Z));
            return new GridMap2DData
            {
                Origin = new Vector2d { x = map.Info.Origin.Position.X, y = map.Info.Origin.Position.Y },
                Size = new Vector2i { x = (int)map.Info.Width, y = (int)map.Info.Height },
                RotationRad = rad,
                RotationMatrix = new Matrix2d(rad),
                Resolution = map.Info.Resolution,
                Data = map.Data.AsSpan().ToArray()
            };
        }

        public static void Convert(IGridMap2DData mapIn, string frameId, IRclNode node, ref OccupancyGrid.Priv mapOut)
        {
            var q = Quaterniond.AxisAngleR(Vector3d.AxisY, mapIn.RotationRad);
            mapOut.Data.CopyFrom(mapIn.Data);
            mapOut.Info.Resolution = (float)mapIn.Resolution;
            mapOut.Info.Origin.Position.X = mapIn.Origin.x;
            mapOut.Info.Origin.Position.Y = mapIn.Origin.y;
            mapOut.Info.Origin.Orientation.X = q.x;
            mapOut.Info.Origin.Orientation.Y = q.y;
            mapOut.Info.Origin.Orientation.Z = q.z;
            mapOut.Info.Origin.Orientation.W = q.w;
            mapOut.Info.Width = (uint)mapIn.Size.x;
            mapOut.Info.Height = (uint)mapIn.Size.y;
            Std.FromData(frameId, node, ref mapOut.Header);
        }
    }
}