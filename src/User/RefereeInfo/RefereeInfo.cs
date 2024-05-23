using AllianceDM.IO;

namespace AllianceDM.PreInfo
{
    // arg[0] = friend outpost hp
    // arg[1] = sentry hp
    // arg[2] = Amount Of Ammunition
    public class RefereeInfo(uint uuid, uint[] revid, string[] args) : Component(uuid, revid, args)
    {
        public float Hp;
        public bool IsInvinciable;
        public bool IsUVALaunch;
        public int AmountOfAmmunition;

        public override void Awake()
        {
            AmountOfAmmunition = 400;
            Hp = 600;
            IsInvinciable = true;
            IsUVALaunch = false;

            IOManager.RegistryMassage(Args[0], (Rosidl.Messages.Std.Int32 msg) => { IsInvinciable = msg.Data > 500; });
            IOManager.RegistryMassage(Args[1], (Rosidl.Messages.Std.Int32 msg) => { Hp = msg.Data; });
            IOManager.RegistryMassage(Args[2], (Rosidl.Messages.Std.Int32 msg) => { AmountOfAmmunition = msg.Data; });
        }

        public override void Update()
        {
            Console.WriteLine(IsInvinciable);
            Console.WriteLine(Hp);

            base.Update();
        }

        public (float Hp, bool Invinciable, bool UVA) Output => (Hp, IsInvinciable, IsUVALaunch);
    }
}