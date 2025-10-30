using System.Numerics;
using Kernel.Core.Messages;
using Kernel.Contract;
using Kernel.Contract.Constraints;
using Kernel.Contract.Geometry;
using Kernel.Contract.Navigation;
using Kernel.Contract.Sensor;
using Kernel.Contract.Tf;
using Kernel.Contract.Visualization;
using Rcl;
using TlarcRosBridge.Infrastructure.Decorators;
using TlarcRosBridge.Infrastructure.Messages.Geometry;
using TlarcRosBridge.Infrastructure.Messages.Nav;
using TlarcRosBridge.Infrastructure.Messages.Sensor;
using TlarcRosBridge.Infrastructure.Messages.Tf2;
using PointCloud = Kernel.Contract.Sensor.PointCloud;

namespace TlarcRosBridge.Infrastructure;

public static class DataProcess
{
    public static class Subscriber
    {
        public static readonly Func<RosMessageBuffer, GridMap2DData> OccupancyGridMapToGridMap2D =
            item =>
                Navigation.GridMap.ConvertToGridMap2DData(
                    ref item.AsRef<OccupancyGrid.Priv>());

        public static readonly Func<RosMessageBuffer, PointCloud> XYZPointCloud =
            item =>
                Sensor.XYZPointCloud(
                    ref item.AsRef<PointCloud2.Priv>());

        public static readonly Func<RosMessageBuffer, PointCloud> FastLioRegistered =
            item =>
                Sensor.FastLioRegistered(
                    ref item.AsRef<PointCloud2.Priv>());


        public static readonly Func<RosMessageBuffer, StdMessage<Vector2>> PointToVector2 =
            item => StdMessage<Vector2>.Build(
                Geometry.WriteData(item.AsRef<PointStamped.Priv>()));


        public static readonly Func<RosMessageBuffer, Kernel.Contract.Geometry.Pose> RawPoseFromPoseStamped =
            item =>
                Geometry.ReadDataWithoutTransform(
                    ref item.AsRef<PoseStamped.Priv>());

        public static readonly Func<RosMessageBuffer, Kernel.Contract.Geometry.Pose> PoseFromPoseStamped =
            item =>
                Geometry.ReadData(
                    ref item.AsRef<PoseStamped.Priv>());
    }

    public static class Publisher
    {
        public static readonly RefAction<GridMap2DData, IRclNode, RosMessageBuffer> GridMap2dToOccupancyGridMap =
            (in GridMap2DData item1, in IRclNode node, ref RosMessageBuffer item2) =>
            {
                Navigation.GridMap.WriteInto(item1, item1.Header.Identifier, node,
                    ref item2.AsRef<OccupancyGrid.Priv>());
            };

        public static readonly RefAction<Kernel.Contract.Tf.TransformStamped, IRclNode, RosMessageBuffer> TransformStampedToTf =
            (in Kernel.Contract.Tf.TransformStamped item1, in IRclNode node, ref RosMessageBuffer item2) =>
            {
                Tf.TlarcTfStampedToTfStamped(item1, node, ref item2.AsRef<Messages.Geometry.TransformStamped.Priv>());
            };

        public static readonly RefAction<TfCollection, IRclNode, RosMessageBuffer> TfCollectionToTfMessage =
            (in TfCollection item1, in IRclNode node, ref RosMessageBuffer item2) =>
            {
                Tf.TfCollectionToTfMessage(item1, node, ref item2.AsRef<TFMessage.Priv>());
            };

        public static readonly RefAction<PointCloud, IRclNode, RosMessageBuffer> PublishPointCloud =
            (in PointCloud item1, in IRclNode node, ref RosMessageBuffer item2) =>
            {
                Sensor.WriteIntoPointCloud(item1, node, ref item2.AsRef<PointCloud2.Priv>());
            };
        public static readonly RefAction<Path2D, IRclNode, RosMessageBuffer> PublishPath =
            (in Path2D item1, in IRclNode node, ref RosMessageBuffer item2) =>
            {
                Navigation.Path.WriteInto(item1, item1.Header.Identifier, node, ref item2.AsRef<Messages.Nav.Path.Priv>());
            };
        public static readonly RefAction<SafeCorridor2DData<Circle>, IRclNode, RosMessageBuffer> PublishCircleSafeCorridor =
            (in SafeCorridor2DData<Circle> item1, in IRclNode node, ref RosMessageBuffer item2) =>
            {
                Visualization.Draw(item1.Header.Identifier, node, item1, ref item2.AsRef<Messages.Visualization.MarkerArray.Priv>());
            };
        public static readonly RefAction<SafeCorridor2DData<Rectangle>, IRclNode, RosMessageBuffer> PublishRectangleSafeCorridor =
            (in SafeCorridor2DData<Rectangle> item1, in IRclNode node, ref RosMessageBuffer item2) =>
            {
                Visualization.Draw(item1.Header.Identifier, node, item1, ref item2.AsRef<Messages.Visualization.MarkerArray.Priv>());
            };
        public static readonly RefAction<Kernel.Contract.Geometry.Pose, IRclNode, RosMessageBuffer> PublishPoseStamped =
            (in Kernel.Contract.Geometry.Pose pose, in IRclNode node, ref RosMessageBuffer item2) =>
                Geometry.WriteData(pose, pose.Header.Identifier, node, ref item2.AsRef<PoseStamped.Priv>());
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