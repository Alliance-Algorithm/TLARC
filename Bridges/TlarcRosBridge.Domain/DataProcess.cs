using Kernel.Core.EventBus;
using Kernel.DataInterfaces.Navigation;
using Rcl;
using TlarcRosBridge.Infrastructure.Messages.Nav;

namespace TlarcRosBridge.Domain;

public static class DataProcess
{
    public static class Subscriber
    {
        public static readonly RefFunc<RosMessageBuffer, IGridMap2DData> OccupancyGridMapToGridMap2D =
            (ref RosMessageBuffer item) =>
                Infrastructure.Decorators.Navigation.GridMap.Convert(ref item.AsRef<OccupancyGrid.Priv>());
    }

    public static class Publisher
    {
        public static readonly RefAction<IGridMap2DData, IRclNode, RosMessageBuffer> GridMap2dToOccupancyGridMap =
            (in IGridMap2DData item1, in IRclNode node, ref RosMessageBuffer item2) =>
            {
                Infrastructure.Decorators.Navigation.GridMap.Convert(item1, "tlarc_link", node,
                    ref item2.AsRef<OccupancyGrid.Priv>());
            };
    }
}