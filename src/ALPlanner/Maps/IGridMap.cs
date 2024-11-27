namespace Maps;

internal interface IGridMap
{
    public Vector3i Index { get; }
    public Vector3d OriginInWorld { get; }
    public Vector3i Size { get; }
    public Vector3d IndexToPositionInWorld(Vector3i position);
    public Vector3i PositionInWorldToIndex(Vector3d position);
    public bool CheckAccessibility(Vector3d from, Vector3d to, float value = 0);
    public bool CheckAccessibility(Vector3i index, float value = 0);
}