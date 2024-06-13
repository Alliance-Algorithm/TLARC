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
    public class UnityNav : GlobalPathFinder
    {
        public string destinationTopicName;
        public string sentryPosTopicName;
        public string forwardTopicName;

        Transform2D SentryPosition;
        Transform2D DestinationPosition;

        IO.ROS2Msgs.Geometry.Pose2D sub_Destination;
        IO.ROS2Msgs.Geometry.Pose2D pub_SentryPos;
        IO.ROS2Msgs.Geometry.Pose2D pub_Forward;

        public override void Start()
        {
            sub_Destination = new();
            pub_SentryPos = new();
            pub_Forward = new();

            sub_Destination.Subscript(destinationTopicName, ((Vector2 pos, float Theta) msg) => { DestPos = msg.pos; });
            pub_SentryPos.RegistetyPublisher(sentryPosTopicName);
            pub_Forward.RegistetyPublisher(forwardTopicName);


        }
        public override void Update()
        {
            pub_SentryPos.Publish((SentryPosition.position, 0));
            pub_Forward.Publish((DestinationPosition.position, 0));
        }

    }
}