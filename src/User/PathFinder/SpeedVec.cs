using System.Numerics;
using AllianceDM.StdComponent;
using Rosidl.Messages.Geometry;

// arg[0] = control velocity topic 
namespace AllianceDM.Nav
{
    public class SpeedVec : Component
    {
        public float maxSpeedDistance;
        public float speedMax;
        public float speedMin;
        public string speedTopicName;
        GlobalPathFinder globalPathFinder;
        Transform2D sentry;
        LocalPathFinder localPathFinder;

        Vector2 Speed;


        IO.ROS2Msgs.Geometry.Pose2D pub_velocity;
        IO.ROS2Msgs.Geometry.Pose2D pub_current;


        public override void Start()
        {
            pub_velocity = new();
            pub_current = new();
            pub_velocity.RegistetyPublisher(speedTopicName);
            pub_current.RegistetyPublisher("/sentry/sensor/velocity");
        }
        public override void Update()
        {
            var fastpos = new Vector2(-sentry.position.X, sentry.position.Y);
            var vec = globalPathFinder.Output - fastpos;
            vec = new(vec.X, vec.Y);
            if (vec.Length() != 0)
                vec = (Math.Clamp(vec.Length() / maxSpeedDistance, 0, 1) * (speedMax - speedMin) + speedMin) * localPathFinder.Output;
            Speed = vec;
            pub_velocity.Publish((Speed, 0));
            pub_current.Publish((Speed, 0));
        }

        public Vector2 Output => Speed;
    }
}