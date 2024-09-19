using TlarcKernel;
namespace TrajectoryTracer.Controller;

class ModelPredictController : Component, IPositionVelocityController
{
    ITrajectory trajectory;
    public Vector3d ControlVolume(Vector3d Position)
    {
        throw new NotImplementedException();
    }
}