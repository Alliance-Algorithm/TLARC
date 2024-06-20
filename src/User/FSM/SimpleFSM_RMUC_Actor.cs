
using System.Numerics;
using AllianceDM.StdComponent;
using Rosidl.Messages.Builtin;

namespace AllianceDM.StateMechines
{
    // args[0] = sentry max time
    // args[1] = angle topic
    public class SimpleFSM_RMUC_Action : Component
    {
        public float maxStayOutTime;
        public string gimbalAngleTopicName;
        Transform2D HiddenPos;
        Transform2D CurisePosMain;
        Transform2D CurisePos2;
        Transform2D CurisePos3;
        Transform2D ControlPos;
        Transform2D RechargeArea;
        SimpleFSM_RMUC fsm;
        Transform2D SentryPos;
        Transform2D SentryTargetRevisePos;
        Transform2D TargetPos;
        float timer;
        float rand;
        bool comeback;
        Vector3 gimbalForward1 = new();
        Vector3 gimbalForward2 = new();
        IO.ROS2Msgs.Geometry.Vector3 pub_angle1;
        IO.ROS2Msgs.Geometry.Vector3 pub_angle2;

        public override void Start()
        {
            rand = DateTime.Now.Second + DateTime.Now.Minute * 60;


            comeback = false;
            pub_angle1 = new();
            pub_angle2 = new();

            pub_angle1.RegistetyPublisher(gimbalAngleTopicName + "1");
            pub_angle2.RegistetyPublisher(gimbalAngleTopicName + "2");
        }
        public override void Update()
        {
            gimbalForward1 = new(0, 0, 0);
            gimbalForward2 = new(0, 0, 0);
            var sentrypos = new Vector2(-SentryPos.position.X, SentryPos.position.Y);
            switch (fsm.Output)
            {
                case Status.Invinciable:
                    comeback = false;
                    timer = DateTime.Now.Second + DateTime.Now.Minute * 60;
                    TargetPos.Set(ControlPos.position);
                    gimbalForward1 = new(0, 1, 1);
                    gimbalForward2 = new(0, -1, 1);
                    break;
                case Status.LowState:
                    // Console.WriteLine((comeback,timer));
                    fsm.healthLine = 300;
                    if (DateTime.Now.Second + DateTime.Now.Minute * 60 - timer > maxStayOutTime - 15)
                        comeback = true;
                    if (comeback)
                    {
                        TargetPos.Set(CurisePosMain.position);
                        if ((sentrypos - CurisePosMain.position +
                         SentryTargetRevisePos.position).Length() < 0.7f)
                        {
                            timer = DateTime.Now.Second + DateTime.Now.Minute * 60;
                            comeback = false;
                        }
                    }
                    else
                        TargetPos.Set(RechargeArea.position);
                    break;
                case Status.Cruise:
                    timer = DateTime.Now.Second + DateTime.Now.Minute * 60;
                    comeback = false;
                    switch ((int)(DateTime.Now.Second + DateTime.Now.Minute * 60 - rand) / 2 % 3)
                    {
                        case 0:
                            TargetPos.Set(CurisePosMain.position);
                            break;
                        case 1:
                            TargetPos.Set(CurisePos2.position);
                            break;
                        case 2:
                            TargetPos.Set(CurisePos3.position);
                            break;
                    }
                    break;
                case Status.Hidden:
                    if (DateTime.Now.Second + DateTime.Now.Minute * 60 - timer > maxStayOutTime - 10)
                        comeback = true;
                    if (comeback)
                    {
                        TargetPos.Set(CurisePosMain.position);
                        if ((sentrypos - CurisePosMain.position +
                         SentryTargetRevisePos.position).Length() < 0.1f)
                            timer = DateTime.Now.Second + DateTime.Now.Minute * 60;
                    }
                    else
                        TargetPos.Set(HiddenPos.position);
                    break;
            }


            pub_angle1.Publish(gimbalForward1);
            pub_angle2.Publish(gimbalForward2);
        }
    }
}