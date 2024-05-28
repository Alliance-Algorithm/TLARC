using System.Numerics;
using AllianceDM.StdComponent;
using Rosidl.Messages.Geometry;

// arg[0] = control velocity topic 
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

        IO.ROS2Msgs.Geometry.Pose2D pub_velocity;
        IO.ROS2Msgs.Geometry.Pose2D pub_current;


        public override void Awake()
        {
            Console.WriteLine(string.Format("AllianceDM.Nav SpeedVec: uuid:{0:D4}", ID));
            global = DecisionMaker.FindComponent<GlobalPathFinder>(id: RecieveID[0]);
            sentry = DecisionMaker.FindComponent<Transform2D>(RecieveID[1]);
            local = DecisionMaker.FindComponent<LocalPathFinder>(RecieveID[2]);

            MaxSpeedDistance = float.Parse(Args[0]);
            SpeedMax = float.Parse(Args[1]);
            SpeedMin = float.Parse(Args[2]);
            pub_velocity = new();
            pub_current = new();
            pub_velocity.RegistetyPublisher(Args[3]);
            pub_current.RegistetyPublisher("/sentry/sensor/velocity");
        }
        public override void Update()
        {
            var fastpos = new Vector2(-sentry.Output.pos.X, sentry.Output.pos.Y);
            var vec = global.Output - fastpos;
            vec = new(vec.X, vec.Y);
            if (vec.Length() != 0)
                vec = (Math.Clamp(vec.Length() / MaxSpeedDistance, 0, 1) * (SpeedMax - SpeedMin) + SpeedMin) * local.Output;
            Speed = vec;
            pub_velocity.Publish((Speed, 0));
            pub_current.Publish((Speed, 0));
        }

        public Vector2 Output => Speed;
    }
}