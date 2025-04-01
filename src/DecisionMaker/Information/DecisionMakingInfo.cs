namespace DecisionMaker.Information;

public class DecisionMakingInfo : Component
{

    string friendOutPostHpTopicName = "/referee/friend/outpost";
    string sentryHpTopicName = "/referee/sentry/hp";
    string enemyBaseHpTopicName = "/referee/enemy_base/hp";
    string sentryBulletCountTopicName = "/referee/sentry/bullet_count";
    // string supportBulletCountTopicName = "/referee/sentry/support_bullet_count";
    string RFIDTopicName = "/referee/rfid";
    string gameStartTopicName = "/referee/game/start";

    public float SentryHp { get; private set; } = SentryHPLimit;
    public float EnemyBaseHp { get; private set; } = BaseHpLimit;
    public int BulletCount { get; private set; } = 400;
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


    IO.ROS2Msgs.Std.Int32 friendOutPostHp;
    IO.ROS2Msgs.Std.Int32 sentryHp;
    IO.ROS2Msgs.Std.Int32 enemyBaseHp;
    IO.ROS2Msgs.Std.Int32 sentryBulletCount;
    // IO.ROS2Msgs.Std.Int32 supportBulletCount;
    IO.ROS2Msgs.Std.Int32 RFID;
    IO.ROS2Msgs.Std.Bool gameStart;

    public override void Start()
    {

        friendOutPostHp = new(IOManager);
        sentryHp = new(IOManager);
        sentryBulletCount = new(IOManager);
        // supportBulletCount = new();
        RFID = new(IOManager);
        gameStart = new(IOManager);
        enemyBaseHp = new(IOManager);

        friendOutPostHp.Subscript(friendOutPostHpTopicName, (int msg) => { FriendOutPostHp = msg; });
        sentryHp.Subscript(sentryHpTopicName, (int msg) => { SentryHp = msg; });
        enemyBaseHp.Subscript(enemyBaseHpTopicName, msg => { EnemyBaseHp = msg; });
        sentryBulletCount.Subscript(sentryBulletCountTopicName, (int msg) => { BulletCount = msg; });
        // supportBulletCount.Subscript(supportBulletCountTopicName, msg => { SupplyRFID = (msg &= (1 << 13) != 0); });
        RFID.Subscript(RFIDTopicName, msg => { SupplyRFID = (msg &= (1 << 13)) != 0; PatrolRFID = (msg &= (1 << 14)) != 0; });
        gameStart.Subscript(gameStartTopicName, _ => { _gameStartTime = DateTime.Now; BulletSupplyCount = 0; });

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

    // Compatible with old versions
    public (float Hp, bool Invincibly, bool UVA) Output => (SentryHp, FriendOutPostHp > 500, false);
}