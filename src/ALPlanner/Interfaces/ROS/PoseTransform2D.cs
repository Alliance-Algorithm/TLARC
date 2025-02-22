using TlarcKernel;

namespace ALPlanner.Interfaces.ROS;

class PoseTransform2D : Transform
{
    IO.ROS2Msgs.Geometry.PoseStampd pose;
    Vector3d offset = Vector3d.Zero;
    public float AngleR;

    string topicName = "/sentry/transform/publish";
    public override void Start()
    {
        pose = new(IOManager);
        pose.Subscript(topicName, x =>
        {
            Position.x = x.pos.X;
            Position.y = x.pos.Y;
            Position += offset;
            AngleR = x.Theta;
        });
    }

    public override void Update()
    {
        // TlarcSystem.LogInfo($"x:{Position.x},y:{Position.y},theta:{Angle}");
    }
}