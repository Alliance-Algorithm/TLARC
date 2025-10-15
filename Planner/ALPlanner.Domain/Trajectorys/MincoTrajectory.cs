using System.Numerics;
using ALPlanner.Infrastructure.Optimizer;
using Kernel.Contract;
using Kernel.Contract.Navigation;
using TlarcRosBridge.Infrastructure.Messages.Builtin;

namespace ALPlanner.Domain.Trajectorys;

public class MincoTrajectory(Minco minco) : ITrajectory2D
{

    public required Header Header { get; init; }
    public DateTime FromWhen
    {
        get => _data.FromWhen; set
        {
            _data.FromWhen = value;
            _data.ToWhen = value + TimeSpan.FromSeconds(minco.TotalSecond);
        }
    }
    public DateTime ToWhen { get; private set; }

    private Trajectory2DHeader _data = new();
    public Trajectory2DHeader Data => _data;

    public Vector2 GetPosition(DateTime time) => minco.GetPosition((time - FromWhen).TotalSeconds);

    public IEnumerable<Vector2> GetPositions(DateTime beginTime, double stepInSecond, int count) =>
        minco.GetPositions((beginTime - FromWhen).TotalSeconds, stepInSecond, count);

    public Vector2 GetVelocity(DateTime time) => time < ToWhen ? minco.GetVelocity((time - FromWhen).TotalSeconds) : minco.GetVelocity(minco.TotalSecond);

}