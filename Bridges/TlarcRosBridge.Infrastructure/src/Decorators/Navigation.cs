using System.Numerics;
using Kernel.Contract;
using Rcl;
using Kernel.Contract.Navigation;
using TlarcRosBridge.Infrastructure.Messages.Nav;

namespace TlarcRosBridge.Infrastructure.Decorators;

internal static class Navigation
{
    public static class GridMap
    {
        public static GridMap2DData ConvertToGridMap2DData(ref OccupancyGrid.Priv map)
        {
            var rad = float.Asin((float)(2 * (map.Info.Origin.Orientation.W * map.Info.Origin.Orientation.Y -
                                              map.Info.Origin.Orientation.X * map.Info.Origin.Orientation.Z)));
            return new GridMap2DData
            {
                Header  = new(){Header = new Header{
                            Identifier = map.Header.FrameId.ToString(),
                            Timestamp  = new(){
                                Second      = map.Header.Stamp.Sec,
                                Nanosecond  = map.Header.Stamp.Nanosec}},
                Origin  = new Vector2 { 
                            X = (float)map.Info.Origin.Position.X, 
                            Y = (float)map.Info.Origin.Position.Y },
                
                Width           = map.Info.Width,
                Height          = map.Info.Height,
                RotationRad     = rad,
                RotationMatrix  = Matrix3x2.CreateRotation(rad),
                Resolution      = map.Info.Resolution,
                },
                Data            = map.Data.AsSpan().ToArray()
            };
        }

        public static void WriteInto(GridMap2DData mapIn,
                                     string frameId,
                                     IRclNode node,
                                     ref OccupancyGrid.Priv mapOut)
        {
            var q = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)mapIn.Header.RotationRad);
            mapOut.Data.CopyFrom(mapIn.Data);
            mapOut.Info.Resolution              = mapIn.Header.Resolution;
            mapOut.Info.Origin.Position.X       = mapIn.Header.Origin.X;
            mapOut.Info.Origin.Position.Y       = mapIn.Header.Origin.Y;
            mapOut.Info.Origin.Orientation.X    = q.X;
            mapOut.Info.Origin.Orientation.Y    = q.Y;
            mapOut.Info.Origin.Orientation.Z    = q.Z;
            mapOut.Info.Origin.Orientation.W    = q.W;
            mapOut.Info.Width                   = mapIn.Header.Width;
            mapOut.Info.Height                  = mapIn.Header.Height;
            Std.FromData(frameId, node, ref mapOut.Header);
        }
    }
    public static class Path
    {


        public static void WriteInto(Path2D pathIn,
                                     string frameId,
                                     IRclNode node,
                                     ref Messages.Nav.Path.Priv pathOut)
        {
            pathOut.Poses = new Messages.Geometry.PoseStamped.PrivSequence(pathIn.Points.Length);
            var index = 0;
            var span = pathOut.Poses.AsSpan();
            foreach (var p in pathIn.Points)
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