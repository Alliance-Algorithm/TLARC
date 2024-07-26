using System.Numerics;
using AllianceDM.IO.ROS2Msgs.RmcsMap;

namespace AllianceDM.PreInfo;

class InformationCommunicators : Component
{
    public string topicName = "/referee/game_status";
    public bool[] Found { get; private set; } = [false, false, false, false, false, false, false];
    public Vector2[] Position { get; private set; } = new Vector2[7];
    public int Locked { get; private set; } = -1;
    public float[] EnemyHp { get; private set; } = new float[7];
    public bool AirSupport { get; private set; } = false;

    public float SentryHp { get; private set; } = 400;
    public int BulletCount { get; private set; } = 300;
    public int BulletSupplyCount { get; private set; } = 0;
    public float FriendOutPostHp { get; private set; } = 1500;
    public bool SupplyRFID { get; private set; } = false;
    public bool PatrolRFID { get; private set; } = false;
    IO.ROS2Msgs.RmcsMap.GameStatus statusReceiver;

    public const float SentinelHPLimit = 400;
    public const float OutpostHPLimit = 1500;

    public override void Start()
    {
        SentryHp = SentinelHPLimit;
        FriendOutPostHp = OutpostHPLimit;
        statusReceiver = new();
        statusReceiver.Subscript(topicName, msg =>
        {
            Found[0] = msg.EnemiesHero?.Position.X != 114514;
            Found[1] = msg.EnemiesEngineer?.Position.X != 114514;
            Found[2] = msg.EnemiesInfantryIii?.Position.X != 114514;
            Found[3] = msg.EnemiesInfantryIv?.Position.X != 114514;
            Found[4] = msg.EnemiesInfantryV?.Position.X != 114514;
            Found[5] = false;
            Found[6] = false;
            Position[0] = Found[0] ? msg.EnemiesHero?.Position ?? throw new Exception() : Position[0];
            Position[1] = Found[1] ? msg.EnemiesEngineer?.Position ?? throw new Exception() : Position[1];
            Position[2] = Found[2] ? msg.EnemiesInfantryIii?.Position ?? throw new Exception() : Position[2];
            Position[3] = Found[3] ? msg.EnemiesInfantryIv?.Position ?? throw new Exception() : Position[3];
            Position[4] = Found[4] ? msg.EnemiesInfantryV?.Position ?? throw new Exception() : Position[4];
            EnemyHp[0] = msg.EnemiesHero?.Hp ?? throw new Exception();
            EnemyHp[1] = msg.EnemiesEngineer?.Hp ?? throw new Exception();
            EnemyHp[2] = msg.EnemiesInfantryIii?.Hp ?? throw new Exception();
            EnemyHp[3] = msg.EnemiesInfantryIv?.Hp ?? throw new Exception();
            EnemyHp[4] = msg.EnemiesInfantryV?.Hp ?? throw new Exception();

            FriendOutPostHp = msg.FriendsOutpostHp;
            SentryHp = msg.SentryHp;

            BulletCount = msg.Bullet;
            BulletSupplyCount = msg.SupplyBullet;

            PatrolRFID = msg.RFID == RFID.Portal;
            SupplyRFID = msg.RFID == RFID.Supply;
        });
    }
}