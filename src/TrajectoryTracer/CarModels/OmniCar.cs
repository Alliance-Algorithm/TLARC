using TlarcKernel;
using TlarcKernel.Extensions.Array;
using TlarcKernel.Transform;

namespace TrajectoryTracer.CarModels;

class OmniCar : Component, ICarModel
{
    Transform sentry;
    ITrajectory trajectory;
    private const float controlCycleTime = 0.1f;
    public double ControlCycleTime => controlCycleTime;
    public int ControlVolumeSize => 2;

    public double[] ObservableVolume => [sentry.Position.x, sentry.Position.y];

    public double[] ReferenceObservationVolume => trajectory.Position.xy.ToArray();

    private static readonly double[,] _matrixA = new double[,] { { 1, 0 }, { 0, 1 } };
    private static readonly double[,] _matrixB = new double[,] { { controlCycleTime, 0 }, { 0, controlCycleTime } };

    public double[,] MatrixA => _matrixA;
    public double[,] MatrixB => _matrixB;

}