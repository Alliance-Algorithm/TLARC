namespace Maps;

interface ISafeCorridorGenerator
{
    const double maxLength = 2;
    public SafeCorridorData Generate(Vector3d[] pointList, double maxLength = maxLength);
}