
namespace AllianceDM.AlPlanner;

internal interface IThreeDimensional
{
    internal float X { get; set; }
    internal float Y { get; set; }
    internal float Z { get; set; }
    private int Sign(float i) => i >= 0 ? 1 : 0;
    sealed internal int Index(float x, float y, float z)
    {
        return Sign(X - x) + Sign(Y - y) << 1 + Sign(Z - z) << 2;
    }
    sealed internal int Index(IThreeDimensional other)
    {
        return Sign(X - other.X) + (Sign(Y - other.Y) << 1) + (Sign(Z - other.Z) << 2);
    }
}