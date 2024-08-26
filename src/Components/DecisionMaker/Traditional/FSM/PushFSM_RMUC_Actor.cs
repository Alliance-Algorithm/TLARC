
using System.Numerics;
using Tlarc.StdComponent;
using Rosidl.Messages.Builtin;

namespace Tlarc.StateMachines
{
    // args[0] = sentry max time
    // args[1] = angle topic
    public class PushFSM_RMUC_Actor : Component
    {
        public float maxStayOutTime;
        public string gimbalAngleTopicName;

        Transform2D hiddenPos;
        Transform2D cruisePosMain;
        Transform2D cruisePos2;
        Transform2D cruisePos3;
        // Transform2D controlPos;
        Transform2D rechargeArea;
        PushFSM_RMUC fsm;
        Transform2D sentry;
        Transform2D sentryTargetRevisePos;
        Transform2D target;
        float timer;
        int rand;
        bool comeback;
        Vector3 gimbalForward1 = new();
        Vector3 gimbalForward2 = new();
        Vector2[] pushPos = [new(7.64f, 1.93f), new(5.14f, -6.71f)];
        int posId = 0;

        IO.ROS2Msgs.Geometry.Vector3 pub_angle1;
        IO.ROS2Msgs.Geometry.Vector3 pub_angle2;

        public override void Start()
        {
            Console.WriteLine(string.Format("AllianceDM.StateMachines PushFSM_RMUC_Actor: uuid:{0:D4}", ID));
            rand = DateTime.Now.Second + DateTime.Now.Minute * 60 + +60 * DateTime.Now.Minute;

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
            var sentryPos = new Vector2(-sentry.Position.X, sentry.Position.Y);
            switch (fsm.state)
            {
                case Status.Invincibly:
                    comeback = false;
                    timer = DateTime.Now.Second + DateTime.Now.Minute * 60;
                    target.Set(pushPos[posId]);
                    posId = Math.Clamp(Math.Abs(DateTime.Now.Second + DateTime.Now.Minute * 60 - rand) / 10 % pushPos.Length, 0, 1);
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
                        target.Set(cruisePosMain.Position);
                        if ((sentryPos - cruisePosMain.Position +
                         sentryTargetRevisePos.Position).Length() < 0.7f)
                        {
                            timer = DateTime.Now.Second + DateTime.Now.Minute * 60;
                            comeback = false;
                        }
                    }
                    else
                        target.Set(rechargeArea.Position);
                    break;
                case Status.Cruise:
                    timer = DateTime.Now.Second + DateTime.Now.Minute * 60;
                    comeback = false;
                    switch (Math.Clamp(Math.Abs((int)(DateTime.Now.Second + DateTime.Now.Minute * 60 - rand) / 2 % 3), 0, 2))
                    {
                        case 0:
                            target.Set(cruisePosMain.Position);
                            break;
                        case 1:
                            target.Set(cruisePos2.Position);
                            break;
                        case 2:
                            target.Set(cruisePos3.Position);
                            break;
                    }
                    break;
                case Status.Hidden:
                    if (DateTime.Now.Second + DateTime.Now.Minute * 60 - timer > maxStayOutTime - 10)
                        comeback = true;
                    if (comeback)
                    {
                        target.Set(cruisePosMain.Position);
                        if ((sentryPos - cruisePosMain.Position +
                         sentryTargetRevisePos.Position).Length() < 0.1f)
                            timer = DateTime.Now.Second + DateTime.Now.Minute * 60;
                    }
                    else
                        target.Set(hiddenPos.Position);
                    break;
            }
            pub_angle1.Publish(gimbalForward1);
            pub_angle2.Publish(gimbalForward2);
        }
    }
}