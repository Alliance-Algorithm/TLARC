using System.Numerics;
using System.Runtime.CompilerServices;
using Kernel.DataInterfaces.Navigation;

namespace ALPlanner.Infrastructure.PathSearcher;

class AStar()
{
    IPath2D Search(Vector2 from, Vector2 to, ISdf2D map) => throw new NotImplementedException();
}

struct AStarNode(in float h, in Vector2 point, in AStarNode parent)
{
    public readonly Vector2 point;
    public readonly float H = h;
    public readonly float G = parent.G + (point - parent.point).Length();

    public readonly float F => H + G;
}