using System.ComponentModel;
using System.Numerics;
using ALPlanner.Interfaces;
using ALPlanner.Interfaces.ROS;
using TlarcKernel.IO.ProcessCommunicateInterfaces;

namespace TrajectoryTracer;

class Tracer : Component
{
  IO.ROS2Msgs.Geometry.Pose2D pose2D;
  IO.ROS2Msgs.Geometry.PoseStampd debugTarget;


  public Vector3d velocity;

  [ComponentReferenceFiled]
  IPositionVelocityController controller;
  PoseTransform2D sentry;
  ALPlanner.ALPlanner aLPlanner;

  public override void Start()
  {
    pose2D = new(IOManager);
    debugTarget = new(IOManager);
    pose2D.RegistryPublisher("/tlarc/control/velocity");
    debugTarget.RegistryPublisher("/debug/control/velocity");
  }

  public override void Update()
  {
    velocity = controller.ControlVolume(sentry.Position);
    var fuck = velocity;
    velocity = Quaterniond.AxisAngleR(Vector3d.AxisZ, -sentry.AngleR) * velocity;

    if ((sentry.Position - aLPlanner.Target).Length < 0.1)
      pose2D.Publish(new());
    else
      pose2D.Publish((new Vector2((float)velocity.x, (float)velocity.y), 0));
    debugTarget.Publish((new Vector2((float)sentry.Position.x, (float)sentry.Position.y), (float)Math.Atan2(fuck.y, fuck.x)));
  }
}
