namespace Maps;

internal interface ISemanticMap : IGridMap
{
    public bool CheckAccessibility(Vector3i fromIndex, Vector3i toIndex);
}