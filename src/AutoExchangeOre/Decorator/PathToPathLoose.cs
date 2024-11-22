using System.Collections.Generic;
using System.IO;

class PathToPathLoose : IPathFinder<(double value, double loose)>
{
    public IPathFinder<double> _pathFinder;
    readonly double _loose;
    public PathToPathLoose(IPathFinder<double> pathFinder, double loose = 0.05f)
    {
        _pathFinder = pathFinder;
        _loose = loose;
    }

    public List<LinkedList<(double value, double loose)>> Search(double[] start, double[] target)
    {
        var result = _pathFinder.Search(start, target);
        var ret = new List<LinkedList<(double, double)>>();
        foreach (var path in result)
        {
            var newPath = new LinkedList<(double, double)>();
            foreach (var node in path)
            {
                newPath.AddLast((node, _loose));
            }
            ret.Add(newPath);
        }
        return ret;
    }
}