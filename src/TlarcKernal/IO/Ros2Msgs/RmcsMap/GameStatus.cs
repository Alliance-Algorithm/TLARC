using System.Collections.Concurrent;
using System.Numerics;
using Tlarc.IO;
using Rcl;

namespace Tlarc.IO.ROS2Msgs.RmcsMap
{
    enum RFID : sbyte
    {
        None = 0,
        Portal = 1,
        Supply = 2
    }
    struct RobotStatusInfo(
        int @Hp,
        Vector2 Position
    )
    {
        public int Hp { get; set; } = Hp;
        public Vector2 Position { get; set; } = Position;

        public static implicit operator RobotStatusInfo(Rosidl.Messages.RmcsMap.RobotStatus info) =>
            new(info.Hp, new((float)info.Pose.Y, -(float)info.Pose.X));
    }
    struct GameStatusInfo(
        int @SentryHp = 0,
        int @Bullet = 0,
        int @SupplyBullet = 0,
        RFID @RFID = 0,
        RobotStatusInfo? @EnemiesHero = null,
        RobotStatusInfo? @EnemiesEngineer = null,
        RobotStatusInfo? @EnemiesInfantryIii = null,
        RobotStatusInfo? @EnemiesInfantryIv = null,
        RobotStatusInfo? @EnemiesInfantryV = null,
        int @EnemiesOutpostHp = 0,
        int @EnemiesBaseHp = 0,
        RobotStatusInfo? @FriendsHero = null,
        RobotStatusInfo? @FriendsEngineer = null,
        RobotStatusInfo? @FriendsInfantryIii = null,
        RobotStatusInfo? @FriendsInfantryIv = null,
        RobotStatusInfo? @FriendsInfantryV = null,
        int @FriendsOutpostHp = 0,
        int @FriendsBaseHp = 0
        )
    {
        public int SentryHp { get; set; } = @SentryHp;
        public int Bullet { get; set; } = @Bullet;
        public int SupplyBullet { get; set; } = @SupplyBullet;
        public RFID RFID { get; set; } = @RFID;
        public RobotStatusInfo? EnemiesHero { get; set; } = @EnemiesHero;
        public RobotStatusInfo? EnemiesEngineer { get; set; } = @EnemiesEngineer;
        public RobotStatusInfo? EnemiesInfantryIii { get; set; } = @EnemiesInfantryIii;
        public RobotStatusInfo? EnemiesInfantryIv { get; set; } = @EnemiesInfantryIv;
        public RobotStatusInfo? EnemiesInfantryV { get; set; } = @EnemiesInfantryV;
        public int EnemiesOutpostHp { get; set; } = @EnemiesOutpostHp;
        public int EnemiesBaseHp { get; set; } = @EnemiesBaseHp;
        public RobotStatusInfo? FriendsHero { get; set; } = @FriendsHero;
        public RobotStatusInfo? FriendsEngineer { get; set; } = @FriendsEngineer;
        public RobotStatusInfo? FriendsInfantryIii { get; set; } = @FriendsInfantryIii;
        public RobotStatusInfo? FriendsInfantryIv { get; set; } = @FriendsInfantryIv;
        public RobotStatusInfo? FriendsInfantryV { get; set; } = @FriendsInfantryV;
        public int FriendsOutpostHp { get; set; } = @FriendsOutpostHp;
        public int FriendsBaseHp { get; set; } = @FriendsBaseHp;
    }
    class GameStatus(IOManager io)
    {
        GameStatusInfo data;
        Action<GameStatusInfo> callback;
        IOManager _ioManager = io;

        static protected bool publishFlag = false;

        ConcurrentQueue<GameStatusInfo> receiveData = new();

        void Subscript()
        {
            if (receiveData.Count == 0)
                return;
            while (receiveData.Count > 1) receiveData.TryDequeue(out _);
            callback(receiveData.Last());
        }
        void Publish()
        {
            // we will not send it 
            return;
        }
        public void Subscript(string topicName, Action<GameStatusInfo> callback)
        {
            this.callback = callback;
            _ioManager.TlarcRosMsgs.Input += Subscript;
            _ioManager.RegistrySubscription(topicName, (Rosidl.Messages.RmcsMap.GameStatus msg) =>
            {
                receiveData.Enqueue(
                    new(
                        SentryHp: msg.FriendsSentry.Hp,
                        Bullet: msg.Bullet,
                        SupplyBullet: msg.SupplyBullet,
                        RFID: (RFID)msg.Rfid,
                        EnemiesHero: msg.EnemiesHero,
                        EnemiesEngineer: msg.EnemiesEngineer,
                        EnemiesInfantryIii: msg.EnemiesInfantryIii,
                        EnemiesInfantryIv: msg.EnemiesInfantryIv,
                        EnemiesInfantryV: msg.EnemiesInfantryV,
                        EnemiesOutpostHp: msg.EnemiesOutpostHp,
                        EnemiesBaseHp: msg.EnemiesBaseHp,
                        FriendsHero: msg.FriendsHero,
                        FriendsEngineer: msg.FriendsEngineer,
                        FriendsInfantryIii: msg.FriendsInfantryIii,
                        FriendsInfantryIv: msg.FriendsInfantryIv,
                        FriendsInfantryV: msg.FriendsInfantryV,
                        FriendsOutpostHp: msg.FriendsOutpostHp,
                        FriendsBaseHp: msg.FriendsBaseHp
                     )
                );
            });
        }
        public void RegistryPublisher(string topicName)
        {
            // we will not send msg
            return;
        }
        public void Publish(GameStatusInfo data)
        {
            this.data = data;
            Publish();
        }
    }
}