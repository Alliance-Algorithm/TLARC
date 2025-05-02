using TlarcKernel;

namespace ALPlanner.Collider;

public interface ICollider
{
  public Vector3d Position { get; }
  public Vector3d Velocity { get; }
}
