using Kernel.DataInterfaces;
using Kernel.DataInterfaces.Geometry;
using Kernel.DataInterfaces.Navigation;
using Kernel.DataInterfaces.Sensor;
using Kernel.DataInterfaces.Tf;
using Rcl;
using TlarcRosBridge.Infrastructure.Messages.Geometry;
using TlarcRosBridge.Infrastructure.Messages.Nav;
using TlarcRosBridge.Infrastructure.Messages.Sensor;
using TlarcRosBridge.Infrastructure.Messages.Tf2;

namespace TlarcRosBridge.Infrastructure;

public static class DataProcess
{
    public static class Subscriber
    {
        public static readonly Func<RosMessageBuffer, IGridMap2DData> OccupancyGridMapToGridMap2D =
            item =>
                Decorators.Navigation.GridMap.ConvertToGridMap2DData(
                    ref item.AsRef<OccupancyGrid.Priv>());

        public static readonly Func<RosMessageBuffer, IPointCloud> RmcsSlamSegmentationPart =
            item =>
                Decorators.Sensor.RmcsSlamSegmentationPart(
                    ref item.AsRef<PointCloud2.Priv>());

        public static readonly Func<RosMessageBuffer, IPointCloud> FastLioRegistered =
            item =>
                Decorators.Sensor.FastLioRegistered(
                    ref item.AsRef<PointCloud2.Priv>());


        public static readonly Func<RosMessageBuffer, IPose> RawPoseFromPoseStamped =
            item =>
                Decorators.Geometry.ReadDataWithoutTransform(
                    ref item.AsRef<PoseStamped.Priv>());
    }

    public static class Publisher
    {
        public static readonly RefAction<IGridMap2DData, IRclNode, RosMessageBuffer> GridMap2dToOccupancyGridMap =
            (in IGridMap2DData item1, in IRclNode node, ref RosMessageBuffer item2) =>
            {
                Decorators.Navigation.GridMap.WriteInto(item1, item1.Header.Identifier, node,
                    ref item2.AsRef<OccupancyGrid.Priv>());
            };

        public static readonly RefAction<ITransformStamped, IRclNode, RosMessageBuffer> TransformStampedToTf =
            (in ITransformStamped item1, in IRclNode node, ref RosMessageBuffer item2) =>
            {
                Decorators.Tf.TlarcTfStampedToTfStamped(item1, node, ref item2.AsRef<TransformStamped.Priv>());
            };

        public static readonly RefAction<ITfCollection, IRclNode, RosMessageBuffer> TfCollectionToTfMessage =
            (in ITfCollection item1, in IRclNode node, ref RosMessageBuffer item2) =>
            {
                Decorators.Tf.TfCollectionToTfMessage(item1, node, ref item2.AsRef<TFMessage.Priv>());
            };

        public static readonly RefAction<IPointCloud, IRclNode, RosMessageBuffer> PublishPointCloud =
            (in IPointCloud item1, in IRclNode node, ref RosMessageBuffer item2) =>
            {
                Decorators.Sensor.WriteIntoPointCloud(item1, node, ref item2.AsRef<PointCloud2.Priv>());
            };
    }
}