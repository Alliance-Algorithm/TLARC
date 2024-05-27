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
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
    public class UnityNavPath(uint uuid, uint[] revid, string[] args) : GlobalPathFinder(uuid, revid, args)
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
    {
        Transform2D SentryPosition;
        Transform2D DestinationPosition;


        IO.ROS2Msgs.Nav.Path sub_Destination;
        IO.ROS2Msgs.Geometry.Pose2D pub_SentryPos;
        IO.ROS2Msgs.Geometry.Pose2D pub_Forward;
        public override void Awake()
        {
            Console.WriteLine(string.Format("AllianceDM.Nav UnityNavPath: uuid:{0:D4}", ID));
            SentryPosition = DecisionMaker.FindComponent<Transform2D>(RecieveID[0]);
            DestinationPosition = DecisionMaker.FindComponent<Transform2D>(RecieveID[1]);


            sub_Destination = new();
            pub_SentryPos = new();
            pub_Forward = new();

            sub_Destination.Subscript(Args[1], (System.Numerics.Vector3[] msg) =>
            {
                var fastpos = new Vector2(-SentryPosition.Output.pos.X, SentryPosition.Output.pos.Y);
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
            pub_SentryPos.RegistetyPublisher(Args[2]);
            pub_Forward.RegistetyPublisher(Args[3]);
        }
        public override void Update()
        {
            pub_SentryPos.Publish((SentryPosition.Output.pos, 0));
            pub_Forward.Publish((DestinationPosition.Output.pos, 0));
        }

    }
}