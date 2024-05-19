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


        public override void Awake()
        {
            SentryPosition = DecisionMaker.FindComponent<Transform2D>(RecieveID[0]);
            DestinationPosition = DecisionMaker.FindComponent<Transform2D>(RecieveID[1]);

            Task.Run(async () =>
            {
                using var pub = Ros2Def.node.CreatePublisher<Pose2D>(Args[2]);
                using var pub2 = Ros2Def.node.CreatePublisher<Pose2D>(Args[3]);
                using var nativeMsg = pub.CreateBuffer();
                using var nativeMsg2 = pub2.CreateBuffer();
                using var timer = Ros2Def.context.CreateTimer(Ros2Def.node.Clock, TimeSpan.FromMilliseconds(value: 1000 / int.Parse(Args[0])));

                while (true)
                {
                    nativeMsg.AsRef<Pose2D.Priv>().X = SentryPosition.Output.pos.X;
                    nativeMsg.AsRef<Pose2D.Priv>().Y = SentryPosition.Output.pos.Y;
                    nativeMsg2.AsRef<Pose2D.Priv>().X = DestinationPosition.Output.pos.X;
                    nativeMsg2.AsRef<Pose2D.Priv>().Y = DestinationPosition.Output.pos.Y;
                    pub.Publish(nativeMsg);
                    pub2.Publish(nativeMsg2);
                    await timer.WaitOneAsync(false);
                }
            });

            IOManager.RegistryMassage(Args[1], (Rosidl.Messages.Nav.Path msg) =>
            {
                DestPos.X = 0;
                DestPos.Y = 0;
                for (int i = 0, k = msg.Poses.Length; i < k; i++)
                {
                    Vector2 p = new((float)msg.Poses[i].Pose.Position.X, (float)msg.Poses[i].Pose.Position.Y);
                    if ((SentryPosition.Output.pos - p).Length() < 0.3f)
                        continue;
                    DestPos = p;
                    break;
                }

            });

        }
        public override void Update()
        {

        }

    }
}