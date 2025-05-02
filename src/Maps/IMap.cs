namespace Maps;

internal interface IMap
{
  public bool CheckAccessibility(Vector3d from, Vector3d to, float value = 0);
  public bool CheckAccessibility(Vector3d index, float value = 0);
}
