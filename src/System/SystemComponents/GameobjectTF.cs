
using System.Numerics;

namespace AllianceDM.StdComponent
{
    public class Transform2D : Component
    {

        public string name;
        public Vector2 position = new(0, 0);
        public double angle = 0;

        public override void Start()
        {

        }

        public override void Update()
        {
        }
        public void Set(Vector2 position, float angle = 0)
        {
            this.position = position;
            this.angle = angle;
        }
    }
    public class Transform2DReceiver : Component
    {

        public string topicName;
        Transform2D transform;
        IO.ROS2Msgs.Geometry.Pose2D pose = new();
        public override void Start()
        {
            pose.Subscript(topicName, msg => transform.Set(msg.pos, msg.Theta));
        }

        public override void Update()
        {
        }
    }
}