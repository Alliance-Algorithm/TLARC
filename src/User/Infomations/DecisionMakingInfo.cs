using AllianceDM.IO;

namespace AllianceDM.PreInfo
{
    // arg[0] = friend outpost hp
    // arg[1] = sentry hp
    public class DecisionMakingInfo : Component
    {

        public string friendOutPostHpTopicName;
        public string sentryHpTopicName;

        public float SentryHp { get; private set; }
        public bool IsUVALaunch { get; private set; }
        public int BulletCount { get; private set; }
        public float DefenseBuff { get; private set; }
        public float FriendOutPostHp { get; private set; }
        public float BaseArmorOpeningCountdown { get; private set; }
        public float GameCountdown { get; private set; }
        public bool SupplyRFID { get; private set; }

        public const float SentinelHPLimit = 400;
        public const float OutpostlHPLimit = 1500;


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
        public (float Hp, bool Invinciable, bool UVA) Output => (SentryHp, FriendOutPostHp > 500, IsUVALaunch);
    }
}