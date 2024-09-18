using g4;

namespace ALPlanner.Interfaces;

interface IPositionDecider
{
    public Vector3d TargetPosition { get; }
}