using System.Numerics;
using ALPlanner.Infrastructure.Optimizer;
using Kernel.DataInterfaces;
using Kernel.DataInterfaces.Navigation;
using TlarcRosBridge.Infrastructure.Messages.Builtin;

namespace ALPlanner.Domain.Trajectorys;

public class MincoTrajectory(Minco minco) : ITrajectory2D
{

    public required IHeader Header { get; init; }
    private DateTime _fromWhen;
    public DateTime FromWhen
    {
        get => _fromWhen; set
        {
            _fromWhen = value;
            ToWhen = _fromWhen + TimeSpan.FromSeconds(minco.TotalSecond);
        }
    }
    public DateTime ToWhen { get; private set; }

    public Vector2 GetPosition(DateTime time) => minco.GetPosition((time - FromWhen).TotalSeconds);

    public IEnumerable<Vector2> GetPositions(DateTime beginTime, double stepInSecond, int count) =>
        minco.GetPositions((beginTime - FromWhen).TotalSeconds, stepInSecond, count);
}