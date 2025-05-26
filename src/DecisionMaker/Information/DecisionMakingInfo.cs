namespace DecisionMaker.Information;

public class DecisionMakingInfo : Component
{
  string hpTopicName = "/referee/robots/hp";
  string bulletCountTopicName = "/referee/shooter/bullet_allowance";
  string gameStageTopicName = "/referee/game/stage";
  string colorTopicName = "/referee/id/color";
  string powerLimitTopicName = "/referee/chassis/power_limit";
  string testModeTopicName = "/rmcs/test_mode";

  public float SentryHp { get; private set; } = SentryHPLimit;
  private float _lastSentryHp = SentryHPLimit;
  public float EnemyBaseHp { get; private set; } = BaseHpLimit;
  public int BulletSupplyCount { get; private set; } = 0;
  public bool SupplyRFID { get; private set; } = false;
  public DateTime GameStartTime => _gameStartTime;
  public const float SentryHPLimit = 400;
  public const float BaseHpLimit = 1500;
  public const float OutpostHPLimit = 1500;
  private long _tick = DateTime.Now.Ticks;
  private DateTime _gameStartTime = DateTime.Now;
  public ushort[] EnemiesHp = [100,100,100,100,100,100,100,100];

  public bool TestMode = false;
  public ushort[] Hp = [];
  public ushort BulletCount = 400;
  public GameStage GameStage = GameStage.STARTED;
  public RobotColor RobotColor;
  public double PowerLimit;

  IO.ROS2Msgs.Std.UInt16MultiArray hpConn;
  IO.ROS2Msgs.Std.UInt16 bulletCountConn;
  IO.ROS2Msgs.Std.UInt8 gameStageConn;
  IO.ROS2Msgs.Std.UInt8 colorConn;
  IO.ROS2Msgs.Std.Float64 powerLimitConn;
  IO.ROS2Msgs.Std.Bool testModeConn;


  public override void Start()
  {
    hpConn = new(IOManager);
    bulletCountConn = new(IOManager);
    gameStageConn = new(IOManager);
    colorConn = new(IOManager);
    powerLimitConn = new(IOManager);
    testModeConn = new(IOManager);
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
    testModeConn.Subscript(
      testModeTopicName,
      msg => TestMode = msg
    );

    _tick = DateTime.Now.Ticks;
  }

  public override void Update()
  {
    _tick = DateTime.Now.Ticks;
    if ((DateTime.Now.Ticks - _tick) / 1e7f >= 60)
      BulletSupplyCount += 100;
    if (SupplyRFID)
      BulletSupplyCount = 0;
    if (Hp.Length == 16)
    {
      SentryHp = Hp[RobotColor == RobotColor.BLUE ? 5 : 13];
      EnemyBaseHp = Hp[RobotColor == RobotColor.BLUE ? 14 : 6];
      EnemiesHp = Hp[RobotColor == RobotColor.BLUE ? 8..16 : 0..8];
    }
  }
}
