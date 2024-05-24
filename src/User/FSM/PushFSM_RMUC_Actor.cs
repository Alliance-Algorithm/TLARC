
using System.Numerics;
using AllianceDM.StdComponent;
using Rosidl.Messages.Builtin;

namespace AllianceDM.StateMechines
{
    public class PushFSM_RMUC_Actor(uint uuid, uint[] revid, string[] args) : Component(uuid, revid, args)
    {
        Transform2D HiddenPos;
        Transform2D CurisePosMain;
        Transform2D CurisePos2;
        Transform2D CurisePos3;
        Transform2D ControlPos;
        Transform2D RechargeArea;
        PushFSM_RMUC fsm;
        Transform2D SentryPos;
        Transform2D SentryTargetRevisePos;
        Transform2D TargetPos;
        float timer;
        int rand;
        bool comeback;
        float maxtime;
        Vector3 gimbalForward1 = new();
        Vector3 gimbalForward2 = new();
        Vector2[] pushPos = [new(3.54f, -6.15f), new(6.07f, 1.97f)];
        int posid = 0;
        public override void Awake()
        {
            ControlPos = DecisionMaker.FindComponent<Transform2D>(RecieveID[0]);
            CurisePosMain = DecisionMaker.FindComponent<Transform2D>(RecieveID[1]);
            CurisePos2 = DecisionMaker.FindComponent<Transform2D>(RecieveID[2]);
            CurisePos3 = DecisionMaker.FindComponent<Transform2D>(RecieveID[3]);
            HiddenPos = DecisionMaker.FindComponent<Transform2D>(RecieveID[4]);
            RechargeArea = DecisionMaker.FindComponent<Transform2D>(RecieveID[5]);
            fsm = DecisionMaker.FindComponent<PushFSM_RMUC>(RecieveID[6]);
            SentryPos = DecisionMaker.FindComponent<Transform2D>(RecieveID[7]);
            SentryTargetRevisePos = DecisionMaker.FindComponent<Transform2D>(RecieveID[8]);
            TargetPos = DecisionMaker.FindComponent<Transform2D>(uint.Parse(Args[0]));
            maxtime = float.Parse(Args[1]);
            rand = DateTime.Now.Second;


            comeback = false;
        }
        public override void Update()
        {
            gimbalForward1 = new(0, 0, 0);
            gimbalForward2 = new(0, 0, 0);
            var sentrypos = new Vector2(-SentryPos.Output.pos.X, -SentryPos.Output.pos.Y);
            switch (fsm.Output)
            {
                case Status.Invinciable:
                    comeback = false;
                    timer = DateTime.Now.Second;
                    TargetPos.Set(pushPos[posid]);
                    posid = Math.Clamp(Math.Abs(DateTime.Now.Second - rand) / 10 % pushPos.Length, 0, 1);
                    gimbalForward1 = new(0, 1, 1);
                    gimbalForward2 = new(0, -1, 1);
                    break;
                case Status.LowState:
                    // Console.WriteLine((comeback,timer));
                    fsm.healthLine = 300;
                    if (DateTime.Now.Second - timer > maxtime - 15)
                        comeback = true;
                    if (comeback)
                    {
                        TargetPos.Set(CurisePosMain.Output.pos);
                        if ((sentrypos - CurisePosMain.Output.pos +
                         SentryTargetRevisePos.Output.pos).Length() < 0.7f)
                        {
                            timer = DateTime.Now.Second;
                            comeback = false;
                        }
                    }
                    else
                        TargetPos.Set(RechargeArea.Output.pos);
                    break;
                case Status.Cruise:
                    timer = DateTime.Now.Second;
                    comeback = false;
                    switch ((int)(DateTime.Now.Second - rand) / 2 % 3)
                    {
                        case 0:
                            TargetPos.Set(CurisePosMain.Output.pos);
                            break;
                        case 1:
                            TargetPos.Set(CurisePos2.Output.pos);
                            break;
                        case 2:
                            TargetPos.Set(CurisePos3.Output.pos);
                            break;
                    }
                    break;
                case Status.Hidden:
                    if (DateTime.Now.Second - timer > maxtime - 10)
                        comeback = true;
                    if (comeback)
                    {
                        TargetPos.Set(CurisePosMain.Output.pos);
                        if ((sentrypos - CurisePosMain.Output.pos +
                         SentryTargetRevisePos.Output.pos).Length() < 0.1f)
                            timer = DateTime.Now.Second;
                    }
                    else
                        TargetPos.Set(HiddenPos.Output.pos);
                    break;
            }


        }
        public override void Echo(string topic, int frameRate)
        {
            Task.Run(async () =>
            {
                using var pub = Ros2Def.node.CreatePublisher<Rosidl.Messages.Geometry.Vector3>(topic + "1");
                using var pub2 = Ros2Def.node.CreatePublisher<Rosidl.Messages.Geometry.Vector3>(topic + "2");
                using var nativeMsg = pub.CreateBuffer();
                using var timer = Ros2Def.context.CreateTimer(Ros2Def.node.Clock, TimeSpan.FromMilliseconds(value: 1000 / frameRate));

                while (true)
                {
                    gimbalForward1 /= gimbalForward1.Length();
                    gimbalForward2 /= gimbalForward2.Length();
                    nativeMsg.AsRef<Rosidl.Messages.Geometry.Vector3.Priv>().X = gimbalForward1.X;
                    nativeMsg.AsRef<Rosidl.Messages.Geometry.Vector3.Priv>().Y = gimbalForward1.Y;
                    nativeMsg.AsRef<Rosidl.Messages.Geometry.Vector3.Priv>().Z = gimbalForward1.Z;
                    pub.Publish(nativeMsg);
                    nativeMsg.AsRef<Rosidl.Messages.Geometry.Vector3.Priv>().X = gimbalForward2.X;
                    nativeMsg.AsRef<Rosidl.Messages.Geometry.Vector3.Priv>().Y = gimbalForward2.Y;
                    nativeMsg.AsRef<Rosidl.Messages.Geometry.Vector3.Priv>().Z = gimbalForward2.Z;
                    pub2.Publish(nativeMsg);
                    await timer.WaitOneAsync(false);
                }
            });
        }
    }
}