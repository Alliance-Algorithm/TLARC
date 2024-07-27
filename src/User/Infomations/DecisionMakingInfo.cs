using AllianceDM.IO;
using Rosidl.Messages.Builtin;

namespace AllianceDM.PreInfo
{
    // arg[0] = friend outpost hp
    // arg[1] = sentry hp
    public class DecisionMakingInfo : Component
    {

        public string friendOutPostHpTopicName = "/referee/friend/outpost";
        public string sentryHpTopicName = "/referee/sentry/hp";
        public string sentryBulletCountTopicName = "/referee/sentry/bullet_count";
        public string supportBulletCountTopicName = "/referee/sentry/support_bullet_count";
        public string sentrySupplyRFIDTopicName = "/referee/sentry/rfid/supply";


        public float SentryHp { get; private set; }
        public int BulletCount { get; private set; }
        public int BulletSupplyCount { get; private set; }
        public float DefenseBuff { get; private set; } = 0.6f;
        public float FriendOutPostHp { get; private set; }
        public float BaseArmorOpeningCountdown { get; private set; } = 40;
        public bool SupplyRFID { get; private set; }
        public bool PatrolRFID { get; private set; }
        public const float SentinelHPLimit = 400;
        public const float OutpostHPLimit = 1500;
        private long _tick = DateTime.Now.Ticks;


        IO.ROS2Msgs.Std.Int32 friendOutPostHp;
        IO.ROS2Msgs.Std.Int32 sentryHp;
        IO.ROS2Msgs.Std.Int32 sentryBulletCount;
        IO.ROS2Msgs.Std.Int32 supportBulletCount;
        IO.ROS2Msgs.Std.Bool sentrySupplyRFID;

        public override void Start()
        {

            friendOutPostHp = new();
            sentryHp = new();
            sentryBulletCount = new();
            supportBulletCount = new();
            sentrySupplyRFID = new();
            friendOutPostHp.Subscript(friendOutPostHpTopicName, (int msg) => { FriendOutPostHp = msg; });
            sentryHp.Subscript(sentryHpTopicName, (int msg) => { SentryHp = msg; });
            sentryBulletCount.Subscript(sentryBulletCountTopicName, (int msg) =>
            {
                BulletCount = msg;
            });
            supportBulletCount.Subscript(supportBulletCountTopicName, (int msg) => { BulletSupplyCount = msg; });
            sentrySupplyRFID.Subscript(sentrySupplyRFIDTopicName, msg => { SupplyRFID = msg; });
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
        }

        // Compatible with old versions
        public (float Hp, bool Invincibly, bool UVA) Output => (SentryHp, FriendOutPostHp > 500, false);
    }
}