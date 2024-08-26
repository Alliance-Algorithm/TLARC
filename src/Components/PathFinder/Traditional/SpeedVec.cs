using System.Numerics;
using Tlarc.StdComponent;
using Rosidl.Messages.Geometry;

// arg[0] = control velocity topic 
namespace Tlarc.Nav
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
            pub_velocity = new(IOManager);
            pub_current = new(IOManager);
            pub_velocity.RegistryPublisher(speedTopicName);
            pub_current.RegistryPublisher("/sentry/sensor/velocity");
        }
        public override void Update()
        {
            var fastPos = new Vector2(-sentry.Position.X, sentry.Position.Y);
            var vec = globalPathFinder.Output - fastPos;
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