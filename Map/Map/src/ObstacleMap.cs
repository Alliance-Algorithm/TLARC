

using Kernel.Core.EventBus;
using Kernel.Contract.Navigation;

namespace Map;

public class ObstacleMap
{
    #region 局部变量
    public string EventGridMapName { get; private set; } = "/tlarc/sdf_map";
    public string EventObstacleName { get; private set; } = "/tlarc/obstacle_map";
    public IObstacle? Obstacle { get; private set; }
    #endregion
    #region 公共设置接口
    public ObstacleMap SetEventGridMapName(in string EventMapName)
    {
        EventGridMapName = EventMapName;
        return this;
    }
    public ObstacleMap SetEventObstacleName(in string EventMapName)
    {
        EventObstacleName = EventMapName;
        return this;
    }

    public ObstacleMap BuildMapRelatedMap()
    {
        // EventBus<IGridMap2D>.Instance.Subscribe(EventGridMapName,
        // map =>
        // {
        //     Obstacle = new Sdf2DRelated()
        //     {
        //         MapData = map
        //     };
        //     EventBus<IObstacle>.Instance.Publish(EventObstacleName, Obstacle);
        // }
        // );
        return this;
    }

    public static ObstacleMap Default => new();
    #endregion


}