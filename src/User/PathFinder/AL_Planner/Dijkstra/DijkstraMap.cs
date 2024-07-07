using System.Numerics;
namespace AllianceDM.ALPlanner;

class DijkstraMap
{
    public Vector2[] _points;
    public int[,] _voronoi;
    public bool[,] _access;
    public float[,] _map;


    public Vector2[] Points => _points;
    public int[,] Voronoi => _voronoi;
    public bool[,] Access => _access;
    public float[,] Map => _map;
}