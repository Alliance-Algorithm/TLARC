using Rcl;
using TlarcRosBridge.Infrastructure.Messages.Geometry;
using Vector3 = System.Numerics.Vector3;
using Quaternion = System.Numerics.Quaternion;
using TlarcRosBridge.Infrastructure.Messages.Std;
using Kernel.Contract.Geometry;

namespace TlarcRosBridge.Infrastructure.Decorators;

internal static class Geometry
{

    public static Kernel.Contract.Geometry.Pose ReadDataWithoutTransform(ref PoseStamped.Priv data) =>
        new()
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

    public static void WriteData(Kernel.Contract.Geometry.Pose dataIn,
                                 ReadOnlySpan<char> frameId,
                                 IRclNode node,
                                 ref PoseStamped.Priv dataOut)
    {
        Std.FromData(frameId, node, ref dataOut.Header);
        Geometry.WriteData(dataIn.Position, dataIn.Orientation, ref dataOut.Pose);
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

    public static void WriteData(Vector3 dataInPos, Quaternion dataInRotation, ref Messages.Geometry.Pose.Priv dataOut)
    {
        Geometry.WriteData(dataInPos, ref dataOut);
        Geometry.WriteData(dataInRotation, ref dataOut);
    }


    public static void WriteData(Vector3 dataIn, ref Messages.Geometry.Pose.Priv dataOut)
    {
        dataOut.Position.X = dataIn.X;
        dataOut.Position.Y = dataIn.Y;
        dataOut.Position.Z = dataIn.Z;
    }

    public static void WriteData(Quaternion dataIn, ref Messages.Geometry.Pose.Priv dataOut)
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