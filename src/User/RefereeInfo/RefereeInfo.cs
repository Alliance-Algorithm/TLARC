namespace AllianceDM.PreInfo
{
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
        }

        public override void Update()
        {
            base.Update();
        }

        public (float Hp, bool Invinciable, bool UVA) Output => (Hp, IsInvinciable, IsUVALaunch);
    }
}