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

        public override void Awake()
        {
            Hp = 600;
            IsInvinciable = true;
            IsUVALaunch = true;

            IOManager.RegistryMassage(Args[0], (Rosidl.Messages.Std.Int32 msg) => { IsInvinciable = msg.Data > 500; });
            IOManager.RegistryMassage(Args[1], (Rosidl.Messages.Std.Int32 msg) => { Hp = msg.Data; });
        }

        public override void Update()
        {
            base.Update();
        }

        public (float Hp, bool Invinciable, bool UVA) Output => (Hp, IsInvinciable, IsUVALaunch);
    }
}