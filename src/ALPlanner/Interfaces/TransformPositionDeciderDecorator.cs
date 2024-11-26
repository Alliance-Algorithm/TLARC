
using ALPlanner.Interfaces.ROS;

namespace ALPlanner.Interfaces;

class TransformPositionDeciderDecorator : Component, IPositionDecider
{
    PoseTransform2D target;
    public Vector3d TargetPosition => target.Position;
}