
using System.Numerics;

namespace Tlarc.StdComponent
{
    public class Transform2D : Component
    {

        public string name;
        public Vector2 FixedupVector = new();
        public Vector2 Position
        { get => positionRaw + FixedupVector; set => positionRaw = value; }
        public Vector2 positionRaw = new();
        Transform2DReceiver receiver = new();
        public double angle = 0;

        public override void Start()
        {

        }

        public override void Update()
        {
            Position = receiver.Position;
            angle = receiver.Angle;
        }
        public void Set(Vector2 position, float angle = 0)
        {
            this.Position = position;
            this.angle = angle;
        }
    }
    public class Transform2DReceiver : Component
    {

        public string topicName;
        public string type = "pose_stamped";
        public Vector2 Position { get; set; } = new();
        public float Angle { get; set; } = new();
        IO.ROS2Msgs.Geometry.Pose2D pose;
        IO.ROS2Msgs.Geometry.PoseStampd poseStampd;
        public override void Start()
        {
            pose = new(IOManager);
            poseStampd = new(IOManager);
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