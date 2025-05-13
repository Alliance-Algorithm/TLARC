using System.ComponentModel;
using ALPlanner.Interfaces.ROS;
using TlarcKernel.IO.ProcessCommunicateInterfaces;

namespace TrajectoryTracer;

class Tracer : Component
{
  IO.ROS2Msgs.Geometry.Pose2D pose2D;

  public Vector3d velocity;

  [ComponentReferenceFiled]
  IPositionVelocityController controller;
  PoseTransform2D sentry;

  public override void Start()
  {
    pose2D = new(IOManager);
    pose2D.RegistryPublisher("/sentry/transform/velocity");
  }

  public override void Update()
  {
    velocity = controller.ControlVolume(sentry.Position);
    velocity = Quaterniond.AxisAngleR(Vector3d.AxisZ, -sentry.AngleR ) * velocity;
    pose2D.Publish((new((float)velocity.x, (float)velocity.y), 0));
  }
}
