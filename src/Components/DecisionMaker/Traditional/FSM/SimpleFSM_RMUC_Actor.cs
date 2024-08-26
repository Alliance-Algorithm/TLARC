
using System.Numerics;
using Tlarc.StdComponent;
using Rosidl.Messages.Builtin;

namespace Tlarc.StateMachines
{
    // args[0] = sentry max time
    // args[1] = angle topic
    public class SimpleFSM_RMUC_Action : Component
    {
        float maxStayOutTime;
        string gimbalAngleTopicName;

        Transform2D HiddenPos;
        Transform2D CruisePosMain;
        Transform2D CruisePos2;
        Transform2D CruisePos3;
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
            pub_angle1 = new(IOManager);
            pub_angle2 = new(IOManager);

            pub_angle1.RegistryPublisher(gimbalAngleTopicName + "1");
            pub_angle2.RegistryPublisher(gimbalAngleTopicName + "2");
        }
        public override void Update()
        {
            gimbalForward1 = new(0, 0, 0);
            gimbalForward2 = new(0, 0, 0);
            var sentryPos = new Vector2(-SentryPos.Position.X, SentryPos.Position.Y);
            switch (fsm.Output)
            {
                case Status.Invincibly:
                    comeback = false;
                    timer = DateTime.Now.Second + DateTime.Now.Minute * 60;
                    TargetPos.Set(ControlPos.Position);
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
                        TargetPos.Set(CruisePosMain.Position);
                        if ((sentryPos - CruisePosMain.Position +
                         SentryTargetRevisePos.Position).Length() < 0.7f)
                        {
                            timer = DateTime.Now.Second + DateTime.Now.Minute * 60;
                            comeback = false;
                        }
                    }
                    else
                        TargetPos.Set(RechargeArea.Position);
                    break;
                case Status.Cruise:
                    timer = DateTime.Now.Second + DateTime.Now.Minute * 60;
                    comeback = false;
                    switch ((int)(DateTime.Now.Second + DateTime.Now.Minute * 60 - rand) / 2 % 3)
                    {
                        case 0:
                            TargetPos.Set(CruisePosMain.Position);
                            break;
                        case 1:
                            TargetPos.Set(CruisePos2.Position);
                            break;
                        case 2:
                            TargetPos.Set(CruisePos3.Position);
                            break;
                    }
                    break;
                case Status.Hidden:
                    if (DateTime.Now.Second + DateTime.Now.Minute * 60 - timer > maxStayOutTime - 10)
                        comeback = true;
                    if (comeback)
                    {
                        TargetPos.Set(CruisePosMain.Position);
                        if ((sentryPos - CruisePosMain.Position +
                         SentryTargetRevisePos.Position).Length() < 0.1f)
                            timer = DateTime.Now.Second + DateTime.Now.Minute * 60;
                    }
                    else
                        TargetPos.Set(HiddenPos.Position);
                    break;
            }


            pub_angle1.Publish(gimbalForward1);
            pub_angle2.Publish(gimbalForward2);
        }
    }
}