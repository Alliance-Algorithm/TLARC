namespace AllianceDM.ALPlanner;
internal class ESDFMapData
{
    public sbyte[,] Map => _staticMap;
    public int[,,] Obstacles => _staticObs;
    public int SizeX => _size_x;
    public int SizeY => _size_y;
    public float Resolution => resolution;

    private int _size_x;
    private int _size_y;
    private float resolution;
    private sbyte[,] _staticMap;
    private int[,,] _staticObs;
}
