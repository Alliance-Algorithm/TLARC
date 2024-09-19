using System.ComponentModel;

namespace TrajectoryTracer;

class Tracer : Component
{
    public Vector3d velocity;
    IPositionVelocityController controller;
    Transform sentry;
    public override void Update()
    {
        velocity = controller.ControlVolume(sentry.Position);
    }
}