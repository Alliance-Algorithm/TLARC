
using System.Numerics;

namespace AllianceDM.StdComponent
{
    public class Transform2D : Component
    {

        public string name;
        public Vector2 FixedupVector = new();
        public Vector2 position
        { get => _position + FixedupVector; set => _position = value; }
        private Vector2 _position;
        Transform2DReceiver receiver = new();
        public double angle = 0;

        public override void Start()
        {

        }

        public override void Update()
        {
            _position = receiver.Position;
            angle = receiver.Angle;
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
        public string type = "pose_stamped";
        public Vector2 Position { get; set; } = new();
        public float Angle { get; set; } = new();
        IO.ROS2Msgs.Geometry.Pose2D pose = new();
        IO.ROS2Msgs.Geometry.PoseStampd poseStampd = new();
        public override void Start()
        {
            if (type == "pose2d")
                pose.Subscript(topicName, msg => { Position = msg.pos; Angle = msg.Theta; });
            else if (type == "pose_stamped")
                poseStampd.Subscript(topicName, msg => { Position = msg.pos; Angle = msg.Theta; });

        }

        public override void Update()
        {
        }
    }
}