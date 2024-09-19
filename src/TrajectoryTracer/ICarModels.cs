namespace TrajectoryTracer;

interface ICarModel
{
    public double[] ControlVolume { get; }
    public double[] ObservableVolume { get; }
    public double[] ReferenceObservationVolume { get; }


    public double[,] MatrixA { get; }
    public double[,] MatrixB { get; }


    public double[] X => ObservableVolume;
    public double[] U => ControlVolume;
    public double[] RefX => ReferenceObservationVolume;

    public double[,] A => MatrixA;
    public double[,] B => MatrixB;
}