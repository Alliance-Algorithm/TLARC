using System.Numerics;
using System.Runtime.CompilerServices;
using Kernel.DataInterfaces;
using Kernel.DataInterfaces.Navigation;

namespace ALPlanner.Infrastructure.PathSearcher;

class Path(string id, int length, IEnumerable<Vector2> path) : IPath2D, IHeader
{
    public string Identifier => id;

    public IHeader Header => this;

    public int Length => length;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<Vector2> GetPoints() => path;
}