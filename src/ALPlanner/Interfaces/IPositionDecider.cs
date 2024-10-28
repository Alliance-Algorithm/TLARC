using TlarcKernel;
namespace ALPlanner.Interfaces;

public interface IPositionDecider
{
    public Vector3d TargetPosition { get; }
}