using System.Numerics;
using AllianceDM.StdComponent;
using Rosidl.Messages.Geometry;

namespace AllianceDM.Nav
{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
    public class SpeedVec(uint uuid, uint[] revid, string[] args) : Component(uuid, revid, args)
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
    {
        GlobalPathFinder global;
        Transform2D sentry;
        LocalPathFinder local;
        float MaxSpeedDistance;
        float SpeedMax;
        float SpeedMin;

        Vector2 Speed;

        public override void Awake()
        {
            global = DecisionMaker.FindComponent<GlobalPathFinder>(id: RecieveID[0]);
            sentry = DecisionMaker.FindComponent<Transform2D>(RecieveID[1]);
            local = DecisionMaker.FindComponent<LocalPathFinder>(RecieveID[2]);

            MaxSpeedDistance = float.Parse(Args[0]);
            SpeedMax = float.Parse(Args[1]);
            SpeedMin = float.Parse(Args[2]);
        }
        public override void Update()
        {
            var vec = global.Output - sentry.Output.pos;
            vec = new(vec.X, vec.Y);
            if (vec.Length() != 0)
                vec = (Math.Clamp(vec.Length() / MaxSpeedDistance, 0, 1) * (SpeedMax - SpeedMin) + SpeedMin) * local.Output;
            Speed = vec;
        }
        public override void Echo(string topic, int frameRate)
        {
            Task.Run(async () =>
            {
                using var pub = Ros2Def.node.CreatePublisher<Pose2D>(topic);
                using var nativeMsg = pub.CreateBuffer();
                using var timer = Ros2Def.context.CreateTimer(Ros2Def.node.Clock, TimeSpan.FromMilliseconds(value: 1000 / frameRate));

                //tmp
                using var pub2 = Ros2Def.node.CreatePublisher<Pose2D>("/sentry/sensor/velocity");
                using var nativeMsg2 = pub2.CreateBuffer();

                while (true)
                {
                    nativeMsg.AsRef<Pose2D.Priv>().X = Speed.X;
                    nativeMsg.AsRef<Pose2D.Priv>().Y = Speed.Y;
                    pub.Publish(nativeMsg);
                    nativeMsg2.AsRef<Pose2D.Priv>().X = local.Output.X;
                    nativeMsg2.AsRef<Pose2D.Priv>().Y = local.Output.Y;
                    pub2.Publish(nativeMsg);
                    await timer.WaitOneAsync(false);
                }
            });
        }

        public Vector2 Output => Speed;
    }
}