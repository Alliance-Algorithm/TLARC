using Kernel.DataInterfaces.Geometry;
using Rcl;
using TlarcRosBridge.Infrastructure.Messages.Geometry;

namespace TlarcRosBridge.Infrastructure.Decorators;

internal static class Geometry
{
    private class PoseInner : IPose
    {
        public System.Numerics.Vector3 Position { get; set; }
        public System.Numerics.Quaternion Orientation { get; set; }
    }

    public static IPose ReadDataWithoutTransform(ref PoseStamped.Priv data) =>
        new PoseInner
        {
            Position = new System.Numerics.Vector3((float)data.Pose.Position.X, (float)data.Pose.Position.Y,
                (float)data.Pose.Position.Z),
            Orientation = new System.Numerics.Quaternion(
                (float)data.Pose.Orientation.X,
                (float)data.Pose.Orientation.Y,
                (float)data.Pose.Orientation.Z,
                (float)data.Pose.Orientation.W
            )
        };

    public static void WriteData(Vector3 dataInPos,
                                 Quaternion dataInRotation,
                                 ReadOnlySpan<char> frameId,
                                 IRclNode node,
                                 ref PoseStamped.Priv dataOut)
    {
        Geometry.WriteData(dataInPos, frameId, node, ref dataOut);
        Geometry.WriteData(dataInRotation, frameId, node, ref dataOut);
    }

    public static void WriteData(Vector3 dataInPos, Quaternion dataInRotation, ref PoseStamped.Priv dataOut)
    {
        Geometry.WriteData(dataInPos, ref dataOut.Pose);
        Geometry.WriteData(dataInRotation, ref dataOut.Pose);
    }

    public static void WriteData(Vector3 dataIn,
                                 ReadOnlySpan<char> frameId,
                                 IRclNode node,
                                 ref PoseStamped.Priv dataOut)
    {
        Std.FromData(frameId, node, ref dataOut.Header);
        Geometry.WriteData(dataIn, ref dataOut.Pose);
    }

    public static void WriteData(Quaternion dataIn,
                                 ReadOnlySpan<char> frameId,
                                 IRclNode node,
                                 ref PoseStamped.Priv dataOut)
    {
        Std.FromData(frameId, node, ref dataOut.Header);
        Geometry.WriteData(dataIn, ref dataOut.Pose);
    }

    public static void WriteData(Vector3 dataIn, ref PoseStamped.Priv dataOut) =>
        Geometry.WriteData(dataIn, ref dataOut.Pose);

    public static void WriteData(Quaternion dataIn, ref PoseStamped.Priv dataOut) =>
        Geometry.WriteData(dataIn, ref dataOut.Pose);

    public static void WriteData(Vector3 dataInPos, Quaternion dataInRotation, ref Pose.Priv dataOut)
    {
        Geometry.WriteData(dataInPos, ref dataOut);
        Geometry.WriteData(dataInRotation, ref dataOut);
    }


    public static void WriteData(Vector3 dataIn, ref Pose.Priv dataOut)
    {
        dataOut.Position.X = dataIn.X;
        dataOut.Position.Y = dataIn.Y;
        dataOut.Position.Z = dataIn.Z;
    }

    public static void WriteData(Quaternion dataIn, ref Pose.Priv dataOut)
    {
        dataOut.Orientation.W = dataIn.W;
        dataOut.Orientation.X = dataIn.X;
        dataOut.Orientation.Y = dataIn.Y;
        dataOut.Orientation.Z = dataIn.Z;
    }

    public static System.Numerics.Vector2 WriteData(PointStamped.Priv dataIn)
    {
        return new((float)dataIn.Point.X, (float)dataIn.Point.Y);
    }
}