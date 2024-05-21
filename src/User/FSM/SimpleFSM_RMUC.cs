using AllianceDM.PreInfo;
using AllianceDM.StdComponent;

// arge[0] = sratus topic
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
        public override void Echo(string topic, int frameRate)
        {
            Task.Run(async () =>
            {
                using var pub = Ros2Def.node.CreatePublisher<Rosidl.Messages.Std.Int32>(topic);
                using var nativeMsg = pub.CreateBuffer();
                using var timer = Ros2Def.context.CreateTimer(Ros2Def.node.Clock, TimeSpan.FromMilliseconds(value: 1000 / frameRate));

                while (true)
                {
                    nativeMsg.AsRef<Rosidl.Messages.Std.Int32.Priv>().Data = (int)state;
                    pub.Publish(nativeMsg);
                    await timer.WaitOneAsync(false);
                }
            });
        }
    }
}