using System.Numerics;
using AllianceDM.IO;
using AllianceDM.StdComponent;
using Rosidl.Messages.Geometry;

// args[0] = FPS
// args[1] = Destination Topic
// args[2] = SentryPosition Public
// args[3] = Forward Public
namespace AllianceDM.Nav
{
    public class UnityNavPath : GlobalPathFinder
    {
        public string unityGlobalPathTopicName;
        public string sentryTopicName;
        public string sentryDestTopicName;

        Transform2D sentry;
        Transform2D destination;


        IO.ROS2Msgs.Nav.Path sub_Destination;
        IO.ROS2Msgs.Geometry.Pose2D pub_SentryPos;
        IO.ROS2Msgs.Geometry.Pose2D pub_Forward;
        public override void Start()
        {
            sub_Destination = new();
            pub_SentryPos = new();
            pub_Forward = new();

            sub_Destination.Subscript(unityGlobalPathTopicName, (System.Numerics.Vector3[] msg) =>
            {
                var fastpos = new Vector2(-sentry.position.X, sentry.position.Y);
                DestPos.X = 0;
                DestPos.Y = 0;
                for (int i = 0, k = msg.Length; i < k; i++)
                {
                    Vector2 p = new(msg[i].X, msg[i].Y);
                    if ((fastpos - p).Length() < 0.3f)
                        continue;
                    DestPos = p;
                    break;
                }
            });
            pub_SentryPos.RegistetyPublisher(sentryTopicName);
            pub_Forward.RegistetyPublisher(sentryDestTopicName);
        }
        public override void Update()
        {
            pub_SentryPos.Publish((sentry.position, 0));
            pub_Forward.Publish((destination.position, 0));
        }

    }
}