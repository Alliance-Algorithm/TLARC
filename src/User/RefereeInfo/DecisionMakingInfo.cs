using AllianceDM.IO;

namespace AllianceDM.PreInfo
{
    // arg[0] = friend outpost hp
    // arg[1] = sentry hp
    public class DecisionMakingInfo : Component
    {

        public string friendOutPostHpTopicName;
        public string sentryHpTopicName;

        float Hp;
        bool IsInvinciable;
        bool IsUVALaunch;

        IO.ROS2Msgs.Std.Int32 friendOutPostHp;
        IO.ROS2Msgs.Std.Int32 sentryHp;

        public override void Start()
        {
            Hp = 400;
            IsInvinciable = true;
            IsUVALaunch = false;

            friendOutPostHp = new();
            sentryHp = new();
            friendOutPostHp.Subscript(friendOutPostHpTopicName, (int msg) => { IsInvinciable = msg > 500; });
            sentryHp.Subscript(sentryHpTopicName, (int msg) => { Hp = msg; });
        }

        public override void Update()
        {
            base.Update();
        }

        public (float Hp, bool Invinciable, bool UVA) Output => (Hp, IsInvinciable, IsUVALaunch);
    }
}