using System.Numerics;
using System.Text;

namespace Tlarc.TrajectoryPlanner.Utils;

class JumpPointMap : Component
{
    private GridMap gridMap;

    private string mapPath;
    private Vector2[] _points;
    private bool[,] _pointConnectMap;
    private int _pointCount;

    public List<Vector2> this[Vector2 pos]
    {
        get { return []; }
        set { }
    }


    public override void Start()
    {
        var rawData = File.ReadAllText(TlarcSystem.RootPath + mapPath, Encoding.ASCII);
        var rawDataInRow = rawData.Split('\n');
        _pointCount = int.Parse(rawDataInRow[0]);
        _points = new Vector2[_pointCount];
        _pointConnectMap = new bool[_pointCount, _pointCount];

        for (int i = 0; i < _pointCount; i++)
        {
            var rawPointData = rawDataInRow[i + 1].Split(' ');
            _points[i].X = float.Parse(rawPointData[0]);
            _points[i].Y = float.Parse(rawPointData[1]);
        }
        for (int i = 0; i < _pointCount; i++)
        {
            var rawConnectData = rawDataInRow[i + 1 + _pointCount].Split(' ');
            for (int j = 0; j < _pointCount; j++)
                _pointConnectMap[i, j] = rawConnectData[j] != "0";
        }
    }
}