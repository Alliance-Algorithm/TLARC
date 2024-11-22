
namespace RapidlyArmPlanner.PathFinder;

interface IPathFinder<T>
{
    List<LinkedList<T>> Search(double[] start, double[] target);
}