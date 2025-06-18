using System.Numerics;

namespace Kernel.DataInterfaces.Navigation;

public interface IPath2D : ITlarcData, ITransform
{
    internal IEnumerable<Vector2> GetPoints();
}