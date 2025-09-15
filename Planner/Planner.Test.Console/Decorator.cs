
using System.Numerics;
using Kernel.DataInterfaces;
using Kernel.DataInterfaces.Navigation;

class PathDecorator<T>(T path, IHeader header) : IPath2D where T : IEnumerable<Vector2>
{
    public IHeader Header => header;

    public int Length => path.Count();

    public IEnumerable<Vector2> GetPoints() => path;
}