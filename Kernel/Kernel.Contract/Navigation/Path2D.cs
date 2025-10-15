using System.Numerics;

namespace Kernel.Contract.Navigation;

public struct Path2D : ITlarcData
{
    public Header       Header;
    public Vector2[]    Points;
}