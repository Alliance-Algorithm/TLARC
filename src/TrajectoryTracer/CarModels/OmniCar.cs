using TlarcKernel;
using TlarcKernel.Extensions.Array;
using TlarcKernel.Transform;
using TrajectoryTracer.Trajectory;

namespace TrajectoryTracer.CarModels;

class OmniCar : Component, ICarModel
{
    [ComponentReferenceFiled]
    Transform sentry;
    [ComponentReferenceFiled]
    ITrajectory<Vector3d> trajectory;
    private const float controlCycleTime = 0.1f;
    public double ControlCycleTime => controlCycleTime;
    public int ControlVolumeSize => 2;

    public double[] ObservableVolume => [sentry.Position.x, sentry.Position.y];

    public double[] ReferenceObservationVolume(int window)
    {
        Vector3d[] vector3Ds = trajectory.Trajectory(controlCycleTime * window, window);
        double[] doubles = new double[vector3Ds.Length * 2];

        if((vector3Ds[0] - sentry.Position).Length < 0.4)
        for (int i = 0, k = vector3Ds.Length; i < k; i++)
        {
            doubles[2 * i] = sentry.Position.x;
            doubles[2 * i + 1] = sentry.Position.y;
        }
        else
        for (int i = 0, k = vector3Ds.Length; i < k; i++)
        {
            doubles[2 * i] = vector3Ds[i].x;
            doubles[2 * i + 1] = vector3Ds[i].y;
        }
        return doubles;
    }

    private static readonly double[,] _matrixA = new double[,] { { 1, 0 }, { 0, 1 } };
    private static readonly double[,] _matrixB = new double[,] { { controlCycleTime, 0 }, { 0, controlCycleTime } };

    public double[,] MatrixA => _matrixA;
    public double[,] MatrixB => _matrixB;

}