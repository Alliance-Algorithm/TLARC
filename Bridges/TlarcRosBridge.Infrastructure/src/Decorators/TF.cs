using Kernel.Contract.Geometry;
using Kernel.Contract.Tf;
using Rcl;
using TlarcRosBridge.Infrastructure.Messages.Geometry;
using TlarcRosBridge.Infrastructure.Messages.Tf2;

namespace TlarcRosBridge.Infrastructure.Decorators;

internal static class Tf
{
    public static void TfCollectionToTfMessage(in TfCollection poseStamped,
                                               in IRclNode node,
                                               ref TFMessage.Priv msg)
    {
        msg.Transforms = new Messages.Geometry.TransformStamped.PrivSequence(poseStamped.TransformStampeds.Length);
        for (var i = 0; i < poseStamped.TransformStampeds.Length; ++i)
            Tf.TlarcTfStampedToTfStamped(poseStamped.TransformStampeds[i], node, ref msg.Transforms.AsSpan()[i]);
    }

    public static void TlarcTfStampedToTfStamped(in Kernel.Contract.Tf.TransformStamped poseStamped,
                                                 in IRclNode node,
                                                 ref Messages.Geometry.TransformStamped.Priv msg)
    {
        msg.ChildFrameId.CopyFrom(poseStamped.Header.Identifier);
        ref var a = ref msg.Transform;
        Std.FromData(poseStamped.ParentFrameId, node, ref msg.Header);
        Tf.WriteIntoTransform(poseStamped.Pose, ref msg.Transform);
    }


    public static void WriteIntoTransform(in Kernel.Contract.Geometry.Pose pose, ref Transform.Priv transform)
    {
        transform.Rotation.X = pose.Orientation.X;
        transform.Rotation.Y = pose.Orientation.Y;
        transform.Rotation.Z = pose.Orientation.Z;
        transform.Rotation.W = pose.Orientation.W;
        transform.Translation.X = pose.Position.X;
        transform.Translation.Y = pose.Position.Y;
        transform.Translation.Z = pose.Position.Z;
    }
}