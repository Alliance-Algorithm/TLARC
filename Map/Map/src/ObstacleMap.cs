

using Kernel.Core.EventBus;
using Kernel.Contract.Navigation;
using SafetyCorridor.Infractructure.CircleSafecorridor;

namespace Map;

public class ObstacleMap
{
    #region 局部变量
    public string EventSdfMapName { get; private set; } = "/tlarc/sdf_map";
    public string EventObstacleName { get; private set; } = "/tlarc/obstacle_map";
    public IObstacle? Obstacle;
    #endregion
    #region 公共设置接口
    public ObstacleMap SetEventSdfMapName(in string EventMapName)
    {
        EventSdfMapName = EventMapName;
        return this;
    }
    public ObstacleMap SetEventObstacleName(in string EventMapName)
    {
        EventObstacleName = EventMapName;
        return this;
    }

    public ObstacleMap BuildMapRelatedMap()
    {
        EventBus<ISdf2D>.Instance.Subscribe(EventSdfMapName,
        map =>
        {
            Obstacle = new Sdf2DRelated()
            {
                MapData = map
            };
            EventBus<IObstacle>.Instance.Publish(EventObstacleName, Obstacle);
        }
        );
        return this;
    }

    public static ObstacleMap Default => new();
    #endregion


}