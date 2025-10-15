using System.Numerics;
using Kernel.Contract.Constraints;

namespace Kernel.Contract.Navigation;

public interface ISdf2D : IMap2D
{
    /// <summary>
    /// when distance = 0 => free
    /// </summary>
    /// <param name="point"></param>
    /// <param name="distance"></param>
    /// <returns></returns>
    public bool IsMoveAble(Vector2 point, out float distance);
}