using System.Numerics;
using Kernel.DataInterfaces.Constraints;

namespace Kernel.DataInterfaces.Navigation;

public interface ISdf2D : IMap2D
{
    public IHeader Header { get; }
    /// <summary>
    /// when distance = 0 => free
    /// </summary>
    /// <param name="point"></param>
    /// <param name="distance"></param>
    /// <returns></returns>
    public bool IsMoveAble(Vector2 point, out float distance);
}