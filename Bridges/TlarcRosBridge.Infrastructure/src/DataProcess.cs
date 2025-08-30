using Kernel.DataInterfaces;
using Kernel.DataInterfaces.Constraints;
using Kernel.DataInterfaces.Geometry;
using Kernel.DataInterfaces.Navigation;
using Kernel.DataInterfaces.Sensor;
using Kernel.DataInterfaces.Tf;
using Rcl;
using TlarcRosBridge.Infrastructure.Decorators;
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
                Navigation.GridMap.ConvertToGridMap2DData(
                    ref item.AsRef<OccupancyGrid.Priv>());

        public static readonly Func<RosMessageBuffer, IPointCloud> RmcsSlamSegmentationPart =
            item =>
                Sensor.RmcsSlamSegmentationPart(
                    ref item.AsRef<PointCloud2.Priv>());

        public static readonly Func<RosMessageBuffer, IPointCloud> FastLioRegistered =
            item =>
                Sensor.FastLioRegistered(
                    ref item.AsRef<PointCloud2.Priv>());


        public static readonly Func<RosMessageBuffer, IPose> RawPoseFromPoseStamped =
            item =>
                Geometry.ReadDataWithoutTransform(
                    ref item.AsRef<PoseStamped.Priv>());
    }

    public static class Publisher
    {
        public static readonly RefAction<IGridMap2DData, IRclNode, RosMessageBuffer> GridMap2dToOccupancyGridMap =
            (in IGridMap2DData item1, in IRclNode node, ref RosMessageBuffer item2) =>
            {
                Navigation.GridMap.WriteInto(item1, item1.Header.Identifier, node,
                    ref item2.AsRef<OccupancyGrid.Priv>());
            };

        public static readonly RefAction<ITransformStamped, IRclNode, RosMessageBuffer> TransformStampedToTf =
            (in ITransformStamped item1, in IRclNode node, ref RosMessageBuffer item2) =>
            {
                Tf.TlarcTfStampedToTfStamped(item1, node, ref item2.AsRef<TransformStamped.Priv>());
            };

        public static readonly RefAction<ITfCollection, IRclNode, RosMessageBuffer> TfCollectionToTfMessage =
            (in ITfCollection item1, in IRclNode node, ref RosMessageBuffer item2) =>
            {
                Tf.TfCollectionToTfMessage(item1, node, ref item2.AsRef<TFMessage.Priv>());
            };

        public static readonly RefAction<IPointCloud, IRclNode, RosMessageBuffer> PublishPointCloud =
            (in IPointCloud item1, in IRclNode node, ref RosMessageBuffer item2) =>
            {
                Sensor.WriteIntoPointCloud(item1, node, ref item2.AsRef<PointCloud2.Priv>());
            };
        public static readonly RefAction<IPath2D, IRclNode, RosMessageBuffer> PublishPath =
            (in IPath2D item1, in IRclNode node, ref RosMessageBuffer item2) =>
            {
                Navigation.Path.WriteInto(item1, item1.Header.Identifier, node, ref item2.AsRef<Messages.Nav.Path.Priv>());
            };
        public static readonly RefAction<ISafeCorridor2D<Circle2D>, IRclNode, RosMessageBuffer> PublishSafeCorridor =
            (in ISafeCorridor2D<Circle2D> item1, in IRclNode node, ref RosMessageBuffer item2) =>
            {
                Visualization.Draw(item1.Header.Identifier, node, item1, ref item2.AsRef<Messages.Visualization.MarkerArray.Priv>());
            };
    }

    public static class Recast
    {
        public static readonly RefAction<IRclNode, string, RosMessageBuffer, RosMessageBuffer>
            OccupancyGrid =
                (in IRclNode node, in string id, in RosMessageBuffer item1, ref RosMessageBuffer item2) =>
                {
                    item2.AsRef<OccupancyGrid.Priv>().CopyFrom(item1.AsRef<OccupancyGrid.Priv>());
                    Std.FromData(id, node, ref item2.AsRef<OccupancyGrid.Priv>().Header);
                };

        public static readonly RefAction<IRclNode, string, RosMessageBuffer, RosMessageBuffer>
            PointCloud2 =
                (in IRclNode node, in string id, in RosMessageBuffer item1, ref RosMessageBuffer item2) =>
                {
                    item2.AsRef<PointCloud2.Priv>().CopyFrom(item1.AsRef<PointCloud2.Priv>());
                    Std.FromData(id, node, ref item2.AsRef<PointCloud2.Priv>().Header);
                };
    }
}