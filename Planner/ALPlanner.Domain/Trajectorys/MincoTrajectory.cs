using System.Numerics;
using ALPlanner.Infrastructure.Optimizer;
using Kernel.DataInterfaces;
using Kernel.DataInterfaces.Navigation;

namespace ALPlanner.Domain.Trajectorys;

public class MincoTrajectory(Minco minco) : ITrajectory2D
{

    public required IHeader Header { get; init; }
    public DateTime FromWhen { get; set; }
    public DateTime ToWhen { get; set; }

    public Vector2 GetPosition(DateTime time) => minco.GetPosition((time - FromWhen).TotalSeconds);

    public IEnumerable<Vector2> GetPositions(DateTime beginTime, double stepInSecond, int count) =>
        minco.GetPositions((beginTime - FromWhen).TotalSeconds, stepInSecond, count);
}