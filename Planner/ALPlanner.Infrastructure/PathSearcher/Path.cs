using System.Numerics;
using System.Runtime.CompilerServices;
using Kernel.DataInterfaces;
using Kernel.DataInterfaces.Navigation;

namespace ALPlanner.Infrastructure.PathSearcher;

class Path(string id, IEnumerable<Vector2> path) : IPath2D, IHeader
{
    public string Identifier => id;

    public IHeader Header => this;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<Vector2> GetPoints() => path;
}