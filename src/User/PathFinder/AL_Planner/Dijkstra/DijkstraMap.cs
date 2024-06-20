using System.Numerics;
namespace AllianceDM.AlPlanner;

class DijkstraMap
{
    private Vector2[] _points;
    private int[,] _voronoi;
    private bool[,] _access;
    private float[,] _map;


    public Vector2[] Points => _points;
    public int[,] Voronoi => _voronoi;
    public bool[,] Access => _access;
    public float[,] Map => _map;
}