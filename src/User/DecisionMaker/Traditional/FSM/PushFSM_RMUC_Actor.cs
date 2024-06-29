
using System.Numerics;
using AllianceDM.StdComponent;
using Rosidl.Messages.Builtin;

namespace AllianceDM.StateMechines
{
    // args[0] = sentry max time
    // args[1] = angle topic
    public class PushFSM_RMUC_Actor : Component
    {
        public float maxStayOutTime;
        public string gimbalAngleTopicName;

        Transform2D hiddenPos;
        Transform2D curisePosMain;
        Transform2D curisePos2;
        Transform2D curisePos3;
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
        int posid = 0;

        IO.ROS2Msgs.Geometry.Vector3 pub_angle1;
        IO.ROS2Msgs.Geometry.Vector3 pub_angle2;

        public override void Start()
        {
            Console.WriteLine(string.Format("AllianceDM.StateMechines PushFSM_RMUC_Actor: uuid:{0:D4}", ID));
            rand = DateTime.Now.Second + DateTime.Now.Minute * 60 + +60 * DateTime.Now.Minute;

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
            var sentrypos = new Vector2(-sentry.position.X, sentry.position.Y);
            switch (fsm.state)
            {
                case Status.Invinciable:
                    comeback = false;
                    timer = DateTime.Now.Second + DateTime.Now.Minute * 60;
                    target.Set(pushPos[posid]);
                    posid = Math.Clamp(Math.Abs(DateTime.Now.Second + DateTime.Now.Minute * 60 - rand) / 10 % pushPos.Length, 0, 1);
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
                        target.Set(curisePosMain.position);
                        if ((sentrypos - curisePosMain.position +
                         sentryTargetRevisePos.position).Length() < 0.7f)
                        {
                            timer = DateTime.Now.Second + DateTime.Now.Minute * 60;
                            comeback = false;
                        }
                    }
                    else
                        target.Set(rechargeArea.position);
                    break;
                case Status.Cruise:
                    timer = DateTime.Now.Second + DateTime.Now.Minute * 60;
                    comeback = false;
                    switch (Math.Clamp(Math.Abs((int)(DateTime.Now.Second + DateTime.Now.Minute * 60 - rand) / 2 % 3), 0, 2))
                    {
                        case 0:
                            target.Set(curisePosMain.position);
                            break;
                        case 1:
                            target.Set(curisePos2.position);
                            break;
                        case 2:
                            target.Set(curisePos3.position);
                            break;
                    }
                    break;
                case Status.Hidden:
                    if (DateTime.Now.Second + DateTime.Now.Minute * 60 - timer > maxStayOutTime - 10)
                        comeback = true;
                    if (comeback)
                    {
                        target.Set(curisePosMain.position);
                        if ((sentrypos - curisePosMain.position +
                         sentryTargetRevisePos.position).Length() < 0.1f)
                            timer = DateTime.Now.Second + DateTime.Now.Minute * 60;
                    }
                    else
                        target.Set(hiddenPos.position);
                    break;
            }
            pub_angle1.Publish(gimbalForward1);
            pub_angle2.Publish(gimbalForward2);
        }
    }
}