namespace MPC.Instructure;

interface ICarModel2D
{

    public double ControlCycleTime { get; }
    public double[] ObservableVolume { get; }
    public double[] ReferenceObservationVolume(int window);

    public double[,] MatrixA { get; }
    public double[,] MatrixB { get; }

    public double[] X => ObservableVolume;
    public double[] RefX(int window) => ReferenceObservationVolume(window);

    public double[,] A => MatrixA;
    public double[,] B => MatrixB;

}