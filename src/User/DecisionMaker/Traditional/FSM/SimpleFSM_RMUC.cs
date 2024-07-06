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
    class SimpleFSM_RMUC : Component
    {
        public Status state { get; set; }
        public int healthLine { get; set; } = 160;

        public string statusPubTopicName;


        DecisionMakingInfo info;
        IO.ROS2Msgs.Std.Int32 pub;

        public override void Start()
        {
            Console.WriteLine(string.Format("AllianceDM.StateMechines SimpleFSM_RMUC: uuid:{0:D4}", ID));

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

        public Status Output => state;
    }
}