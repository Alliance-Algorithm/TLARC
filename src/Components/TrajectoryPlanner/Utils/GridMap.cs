using System.Numerics;

namespace Tlarc.TrajectoryPlanner.Utils;

public abstract class GridMap : Component
{
    public const float UnReachable = float.NegativeInfinity;
    /// <summary>
    /// 遵循 价值0~1 ，靠近障碍物值更小，远离障碍物值更大
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public abstract float Cost(Vector2 position);
    public abstract float this[Vector2 position] { get; set; }
    public abstract (int x, int y) PositionToGridIndex(Vector2 position);
    public abstract Vector2 GridIndexToPosition(int x, int y);
    public abstract (int X, int Y) Size { get; }
}