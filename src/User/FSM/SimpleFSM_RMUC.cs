using AllianceDM.PreInfo;
using AllianceDM.StdComponent;

// arge[0] = sratus topic
namespace AllianceDM.StateMechines
{
    internal enum Status
    {
        Invinciable = 0,
        LowState,
        Hidden,
        Cruise
    }
    class SimpleFSM_RMUC(uint uuid, uint[] revid, string[] args) : Component(uuid, revid, args)
    {
        public int healthLine = 160;

        RefereeInfo info;
        Status state;
        IO.ROS2Msgs.Std.Int32 pub;

        public override void Awake()
        {
            info = DecisionMaker.FindComponent<RefereeInfo>(RecieveID[0]);
            state = Status.Invinciable;
            pub.RegistetyPublisher(Args[0]);
        }


        public override void Update()
        {
            switch (state)
            {
                case Status.Invinciable:
                    if (!info.Output.Invinciable)
                        state = Status.Cruise;
                    break;
                case Status.LowState:
                    if (info.Output.Hp >= 300)
                        state = Status.Cruise;
                    break;
                case Status.Hidden:
                    if (!info.Output.UVA)
                        state = Status.Cruise;
                    break;
                case Status.Cruise:
                    if (info.Output.UVA)
                        state = Status.Hidden;
                    else if (info.Output.Hp < healthLine)
                        state = Status.LowState;
                    break;
                default:
                    break;
            }
            pub.Publish((int)state);
        }

        public Status Output => state;
    }
}