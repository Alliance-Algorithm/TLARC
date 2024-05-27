
using System.Numerics;
using AllianceDM.StdComponent;
using Rosidl.Messages.Builtin;

namespace AllianceDM.StateMechines
{
    // args[0] = sentry max time
    // args[1] = angle topic
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
        Vector2[] pushPos = [new(7.64f, 1.93f), new(5.14f, -6.71f)];
        int posid = 0;

        IO.ROS2Msgs.Geometry.Vector3 pub_angle1;
        IO.ROS2Msgs.Geometry.Vector3 pub_angle2;

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
            TargetPos = DecisionMaker.FindComponent<Transform2D>(RecieveID[9]);
            maxtime = float.Parse(Args[0]);
            rand = DateTime.Now.Second + DateTime.Now.Minute * 60 + +60 * DateTime.Now.Minute;


            comeback = false;

            pub_angle1 = new();
            pub_angle2 = new();

            pub_angle1.RegistetyPublisher(Args[1] + "1");
            pub_angle2.RegistetyPublisher(Args[1] + "2");
        }
        public override void Update()
        {
            gimbalForward1 = new(0, 0, 0);
            gimbalForward2 = new(0, 0, 0);
            var sentrypos = new Vector2(-SentryPos.Output.pos.X, SentryPos.Output.pos.Y);
            switch (fsm.state)
            {
                case Status.Invinciable:
                    comeback = false;
                    timer = DateTime.Now.Second + DateTime.Now.Minute * 60;
                    TargetPos.Set(pushPos[posid]);
                    posid = Math.Clamp(Math.Abs(DateTime.Now.Second + DateTime.Now.Minute * 60 - rand) / 10 % pushPos.Length, 0, 1);
                    gimbalForward1 = new(0, 1, 1);
                    gimbalForward2 = new(0, -1, 1);
                    break;
                case Status.LowState:
                    // Console.WriteLine((comeback,timer));
                    fsm.healthLine = 300;
                    if (DateTime.Now.Second + DateTime.Now.Minute * 60 - timer > maxtime - 15)
                        comeback = true;


                    if (comeback)
                    {
                        TargetPos.Set(CurisePosMain.Output.pos);
                        if ((sentrypos - CurisePosMain.Output.pos +
                         SentryTargetRevisePos.Output.pos).Length() < 0.7f)
                        {
                            timer = DateTime.Now.Second + DateTime.Now.Minute * 60;
                            comeback = false;
                        }
                    }
                    else
                        TargetPos.Set(RechargeArea.Output.pos);
                    break;
                case Status.Cruise:
                    timer = DateTime.Now.Second + DateTime.Now.Minute * 60;
                    comeback = false;
                    switch (Math.Clamp(Math.Abs((int)(DateTime.Now.Second + DateTime.Now.Minute * 60 - rand) / 2 % 3), 0, 2))
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
                    if (DateTime.Now.Second + DateTime.Now.Minute * 60 - timer > maxtime - 10)
                        comeback = true;
                    if (comeback)
                    {
                        TargetPos.Set(CurisePosMain.Output.pos);
                        if ((sentrypos - CurisePosMain.Output.pos +
                         SentryTargetRevisePos.Output.pos).Length() < 0.1f)
                            timer = DateTime.Now.Second + DateTime.Now.Minute * 60;
                    }
                    else
                        TargetPos.Set(HiddenPos.Output.pos);
                    break;
            }
            pub_angle1.Publish(gimbalForward1);
            pub_angle2.Publish(gimbalForward2);
        }
    }
}