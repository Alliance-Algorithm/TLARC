using AllianceDM.IO;

namespace AllianceDM.PreInfo
{
    // arg[0] = friend outpost hp
    // arg[1] = sentry hp
    public class RefereeInfo(uint uuid, uint[] revid, string[] args) : Component(uuid, revid, args)
    {
        float Hp;
        bool IsInvinciable;
        bool IsUVALaunch;

        IO.ROS2Msgs.Std.Int32 friendOutPostHp;
        IO.ROS2Msgs.Std.Int32 sentryHp;

        public override void Awake()
        {
            Hp = 400;
            IsInvinciable = true;
            IsUVALaunch = false;

            friendOutPostHp = new();
            friendOutPostHp.Subscript(Args[0], (int msg) => { IsInvinciable = msg > 500; });
            sentryHp = new();
            sentryHp.Subscript(Args[0], (int msg) => { Hp = msg; });
        }

        public override void Update()
        {
            base.Update();
        }

        public (float Hp, bool Invinciable, bool UVA) Output => (Hp, IsInvinciable, IsUVALaunch);
    }
}