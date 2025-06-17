using g4;
using Rcl;
using TlarcRosBridge.Infrastructure.Messages.Geometry;

namespace TlarcRosBridge.Infrastructure.Decorators;

public static class Geometry
{
    public static void WriteData(Vector3d             dataInPos,
                                 Quaterniond          dataInRotation,
                                 ReadOnlySpan<char>   frameId,
                                 IRclNode             node,
                                 ref PoseStamped.Priv dataOut)
    {
        Geometry.WriteData(dataInPos,      frameId, node, ref dataOut);
        Geometry.WriteData(dataInRotation, frameId, node, ref dataOut);
    }

    public static void WriteData(Vector3d dataInPos, Quaterniond dataInRotation, ref PoseStamped.Priv dataOut)
    {
        Geometry.WriteData(dataInPos,      ref dataOut.Pose);
        Geometry.WriteData(dataInRotation, ref dataOut.Pose);
    }

    public static void WriteData(Vector3d             dataIn,
                                 ReadOnlySpan<char>   frameId,
                                 IRclNode             node,
                                 ref PoseStamped.Priv dataOut)
    {
        Std.FromData(frameId, node, ref dataOut.Header);
        Geometry.WriteData(dataIn, ref dataOut.Pose);
    }

    public static void WriteData(Quaterniond          dataIn,
                                 ReadOnlySpan<char>   frameId,
                                 IRclNode             node,
                                 ref PoseStamped.Priv dataOut)
    {
        Std.FromData(frameId, node, ref dataOut.Header);
        Geometry.WriteData(dataIn, ref dataOut.Pose);
    }

    public static void WriteData(Vector3d dataIn, ref PoseStamped.Priv dataOut)
    {
        Geometry.WriteData(dataIn, ref dataOut.Pose);
    }

    public static void WriteData(Quaterniond dataIn, ref PoseStamped.Priv dataOut)
    {
        Geometry.WriteData(dataIn, ref dataOut.Pose);
    }

    public static void WriteData(Vector3d dataInPos, Quaterniond dataInRotation, ref Pose.Priv dataOut)
    {
        Geometry.WriteData(dataInPos,      ref dataOut);
        Geometry.WriteData(dataInRotation, ref dataOut);
    }


    public static void WriteData(Vector3d dataIn, ref Pose.Priv dataOut)
    {
        dataOut.Position.X = dataIn.x;
        dataOut.Position.Y = dataIn.y;
        dataOut.Position.Z = dataIn.z;
    }

    public static void WriteData(Quaterniond dataIn, ref Pose.Priv dataOut)
    {
        dataOut.Orientation.W = dataIn.w;
        dataOut.Orientation.X = dataIn.x;
        dataOut.Orientation.Y = dataIn.y;
        dataOut.Orientation.Z = dataIn.z;
    }
}
