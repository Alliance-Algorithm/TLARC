using System.Numerics;

namespace Kernel.DataInterfaces.Navigation;

public interface IPath2D : ITlarcData
{
    public IHeader Header { get; }
    public IEnumerable<Vector2> GetPoints();
}