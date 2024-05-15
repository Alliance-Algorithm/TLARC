using AllianceDM.PreInfo;
using AllianceDM.StdComponent;

namespace AllianceDM.StateMechines
{
    internal enum Status
    {
        Invinciable,
        LowState,
        Hidden,
        Curise
    }
    class SimpleFSM_RMUC(uint uuid, uint[] revid, string[] args) : Component(uuid, revid, args)
    {
        RefereeInfo info;
        Status state;

        public override void Awake()
        {
            info = DecisionMaker.FindComponent<RefereeInfo>(RecieveID[0]);
            state = Status.Invinciable;
        }
        public override void Update()
        {
            switch (state)
            {
                case Status.Invinciable:
                    if (!info.Output.Invinciable)
                        state = Status.Curise;
                    break;
                case Status.LowState:
                    if (info.Output.Hp >= 590)
                        state = Status.Curise;
                    break;
                case Status.Hidden:
                    if (!info.Output.UVA)
                        state = Status.Curise;
                    break;
                case Status.Curise:
                    if (info.Output.UVA)
                        state = Status.Hidden;
                    else if (info.Output.Hp < 500)
                        state = Status.LowState;
                    break;
                default:
                    break;
            }
        }

        public Status Output => state;
    }
}