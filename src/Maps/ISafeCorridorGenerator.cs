namespace Maps;

interface ISafeCorridorGenerator
{
    public SafeCorridorData Generate(Vector3d[] pointList);
}