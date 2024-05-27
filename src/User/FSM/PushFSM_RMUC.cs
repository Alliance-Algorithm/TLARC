using AllianceDM.PreInfo;
using AllianceDM.StdComponent;

// args[0] = sratus topic
namespace AllianceDM.StateMechines
{
    class PushFSM_RMUC(uint uuid, uint[] revid, string[] args) : Component(uuid, revid, args)
    {
        public int healthLine = 160;

        RefereeInfo info;
        public Status state;
        IO.ROS2Msgs.Std.Int32 pub;
        public override void Awake()
        {
            Console.WriteLine(string.Format("AllianceDM.StateMechines PushFSM_RMUC: uuid:{0:D4}", ID));
            info = DecisionMaker.FindComponent<RefereeInfo>(RecieveID[0]);
            state = Status.Invinciable;
            pub = new();
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

    }
}