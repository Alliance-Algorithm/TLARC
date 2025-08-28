using System.Numerics;
using Kernel.DataInterfaces;
using Rcl;
using Kernel.DataInterfaces.Navigation;
using TlarcRosBridge.Infrastructure.Messages.Nav;

namespace TlarcRosBridge.Infrastructure.Decorators;

internal static class Navigation
{
    public static class GridMap
    {
        private class GridMap2DData : IGridMap2DData
        {
            public struct HeaderInner(string id) : IHeader
            {
                public string Identifier { get; set; } = id;
            }

            public required IHeader Header { get; init; }
            public required Vector2 Origin { get; init; }
            public required uint Height { get; init; }
            public uint Width { get; init; }
            public required double RotationRad { get; init; }
            public required Matrix3x2 RotationMatrix { get; init; }
            public required float Resolution { get; init; }
            public required sbyte[] Data { get; init; }
        }

        public static IGridMap2DData ConvertToGridMap2DData(ref OccupancyGrid.Priv map)
        {
            var rad = float.Asin((float)(2 * (map.Info.Origin.Orientation.W * map.Info.Origin.Orientation.Y -
                                              map.Info.Origin.Orientation.X * map.Info.Origin.Orientation.Z)));
            return new GridMap2DData
            {
                Header = new GridMap2DData.HeaderInner(map.Header.FrameId.ToString()),
                Origin = new Vector2
                { X = (float)map.Info.Origin.Position.X, Y = (float)map.Info.Origin.Position.Y },
                Width = map.Info.Width,
                Height = map.Info.Height,
                RotationRad = rad,
                RotationMatrix = Matrix3x2.CreateRotation(rad),
                Resolution = map.Info.Resolution,
                Data = map.Data.AsSpan().ToArray()
            };
        }

        public static void WriteInto(IGridMap2DData mapIn,
                                     string frameId,
                                     IRclNode node,
                                     ref OccupancyGrid.Priv mapOut)
        {
            var q = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)mapIn.RotationRad);
            mapOut.Data.CopyFrom(mapIn.Data);
            mapOut.Info.Resolution = mapIn.Resolution;
            mapOut.Info.Origin.Position.X = mapIn.Origin.X;
            mapOut.Info.Origin.Position.Y = mapIn.Origin.Y;
            mapOut.Info.Origin.Orientation.X = q.X;
            mapOut.Info.Origin.Orientation.Y = q.Y;
            mapOut.Info.Origin.Orientation.Z = q.Z;
            mapOut.Info.Origin.Orientation.W = q.W;
            mapOut.Info.Width = mapIn.Width;
            mapOut.Info.Height = mapIn.Height;
            Std.FromData(frameId, node, ref mapOut.Header);
        }
    }
    public static class Path
    {


        public static void WriteInto(IPath2D pathIn,
                                     string frameId,
                                     IRclNode node,
                                     ref Messages.Nav.Path.Priv pathOut)
        {
            pathOut.Poses = new Messages.Geometry.PoseStamped.PrivSequence(pathIn.Length);
            var index = 0;
            var span = pathOut.Poses.AsSpan();
            foreach (var p in pathIn.GetPoints())
            {
                Std.FromData(frameId, node, ref span[index].Header);
                span[index].Pose.Position.X = p.X;
                span[index].Pose.Position.Y = p.Y;
                index++;
            }
            Std.FromData(frameId, node, ref pathOut.Header);
        }
    }
}