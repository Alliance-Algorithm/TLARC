using Tlarc.PreInfo;
using Tlarc.StdComponent;

// args[0] = status topic
namespace Tlarc.StateMachines
{
    internal enum Status
    {
        Invincibly = 0,
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
            Console.WriteLine(string.Format("AllianceDM.StateMachines SimpleFSM_RMUC: uuid:{0:D4}", ID));

            state = Status.Invincibly;
            pub = new(IOManager);
            pub.RegistryPublisher(statusPubTopicName);
        }


        public override void Update()
        {
            switch (state)
            {
                case Status.Invincibly:
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