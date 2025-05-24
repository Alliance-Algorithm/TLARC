namespace DecisionMaker.Information;

public class DecisionMakingInfo : Component
{
  string hpTopicName = "/referee/robots/hp";
  string bulletCountTopicName = "/referee/shooter/bullet_allowance";
  string gameStageTopicName = "/referee/game/stage";
  string colorTopicName = "/referee/id/color";
  string powerLimitTopicName = "/referee/chassis/power_limit";

  public float SentryHp { get; private set; } = SentryHPLimit;
  public float EnemyBaseHp { get; private set; } = BaseHpLimit;
  public int BulletSupplyCount { get; private set; } = 0;
  public float DefenseBuff { get; private set; } = 0.6f;
  public float FriendOutPostHp { get; private set; } = OutpostHPLimit;
  public float BaseArmorOpeningCountdown { get; private set; } = 40;
  public bool SupplyRFID { get; private set; } = false;
  public DateTime GameStartTime => _gameStartTime;
  public bool PatrolRFID { get; private set; } = true;
  public const float SentryHPLimit = 400;
  public const float BaseHpLimit = 1500;
  public const float OutpostHPLimit = 1500;
  private long _tick = DateTime.Now.Ticks;
  private DateTime _gameStartTime = DateTime.Now;

  public ushort[] Hp = [];
  public ushort BulletCount = 400;
  public GameStage GameStage;
  public RobotColor RobotColor;
  public double PowerLimit;

  IO.ROS2Msgs.Std.UInt16MultiArray hpConn;
  IO.ROS2Msgs.Std.UInt16 bulletCountConn;
  IO.ROS2Msgs.Std.UInt8 gameStageConn;
  IO.ROS2Msgs.Std.UInt8 colorConn;
  IO.ROS2Msgs.Std.Float64 powerLimitConn;


  public override void Start()
  {
    hpConn = new(IOManager);
    bulletCountConn = new(IOManager);
    gameStageConn = new(IOManager);
    colorConn = new(IOManager);
    powerLimitConn = new(IOManager);

    hpConn.Subscript(
      hpTopicName,
      msg => Hp = msg
    );
    bulletCountConn.Subscript(
      bulletCountTopicName,
      msg => BulletCount = msg
    );
    gameStageConn.Subscript(
      gameStageTopicName,
     msg => GameStage = (GameStage)msg
    );
    colorConn.Subscript(
      colorTopicName,
      msg => RobotColor = (RobotColor)msg
    );
    powerLimitConn.Subscript(
      powerLimitTopicName,
      msg => PowerLimit = msg
    );

    _tick = DateTime.Now.Ticks;
  }

  public override void Update()
  {
    if (FriendOutPostHp > 0 || PatrolRFID)
      BaseArmorOpeningCountdown = 40;
    else if (!PatrolRFID)
    {
      BaseArmorOpeningCountdown -= (DateTime.Now.Ticks - _tick) / 1e7f;
    }
    _tick = DateTime.Now.Ticks;
    if ((DateTime.Now.Ticks - _tick) / 1e7f >= 60)
      BulletSupplyCount += 100;
    if (SupplyRFID)
      BulletSupplyCount = 0;
  }
}
