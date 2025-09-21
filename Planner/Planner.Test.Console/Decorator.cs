
using System.Numerics;
using Kernel.DataInterfaces;
using Kernel.DataInterfaces.Geometry;
using Kernel.DataInterfaces.Navigation;

class PathDecorator<T>(T path, IHeader _header) : IPath2D where T : IEnumerable<Vector2>
{
    public IHeader Header => _header;
    public int Length => path.Count();

    public IEnumerable<Vector2> GetPoints() => path;
}

record PoseDecoratior(Vector3 Position, Quaternion Orientation, IHeader Header) : IPose;