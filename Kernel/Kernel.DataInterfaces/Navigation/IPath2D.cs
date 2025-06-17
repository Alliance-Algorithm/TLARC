using g4;

namespace Kernel.DataInterfaces.Navigation;

public interface IPath2D : ITlarcData
{
    internal IEnumerable<Vector2d> GetPoints();
}
