using AllianceDM.PreInfo;
using AllianceDM.StdComponent;

// args[0] = sratus topic
namespace AllianceDM.StateMechines
{
    class PushFSM_RMUC : Component
    {
        public int healthLine { get; set; } = 160;
        public Status state { get; set; }

        public string statusPubTopicName;

        DecisionMakingInfo info;
        IO.ROS2Msgs.Std.Int32 pub;
        public override void Start()
        {
            state = Status.Invinciable;
            pub = new();
            pub.RegistetyPublisher(statusPubTopicName);
        }


        public override void Update()
        {
            switch (state)
            {
                case Status.Invinciable:
                    if (!info.Output.Invincibly)
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