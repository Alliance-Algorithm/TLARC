using AllianceDM.IO;

namespace AllianceDM.PreInfo
{
    // arg[0] = friend outpost hp
    // arg[1] = sentry hp
    public class DecisionMakingInfo : Component
    {

        public string friendOutPostHpTopicName = "/referee/friend/outpost";
        public string sentryHpTopicName = "/referee/hp";

        public float SentryHp { get; private set; } = 400;
        public bool IsUVALaunch { get; private set; } = false;
        public int BulletCount { get; private set; } = 300;
        public int BulletSupplCount { get; private set; } = 0;
        public float DefenseBuff { get; private set; } = 0.6f;
        public float FriendOutPostHp { get; private set; } = 1500;
        public float BaseArmorOpeningCountdown { get; private set; } = 0;
        public float GameCountdown { get; private set; } = 500;
        public bool SupplyRFID { get; private set; } = false;

        public const float SentinelHPLimit = 400;
        public const float OutpostHPLimit = 1500;


        IO.ROS2Msgs.Std.Int32 friendOutPostHp;
        IO.ROS2Msgs.Std.Int32 sentryHp;

        public override void Start()
        {
            SentryHp = SentinelHPLimit;
            IsUVALaunch = false;

            friendOutPostHp = new();
            sentryHp = new();
            friendOutPostHp.Subscript(friendOutPostHpTopicName, (int msg) => { FriendOutPostHp = msg; });
            sentryHp.Subscript(sentryHpTopicName, (int msg) => { SentryHp = msg; });
        }

        public override void Update()
        {
            base.Update();
        }

        // Compatible with old versions
        public (float Hp, bool Invincibly, bool UVA) Output => (SentryHp, FriendOutPostHp > 500, IsUVALaunch);
    }
}