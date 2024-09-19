namespace TrajectoryTracer;

interface ICarModel
{
    public double ControlCycleTime { get; }
    public double[] ObservableVolume { get; }
    public double[] ReferenceObservationVolume { get; }
    public int ControlVolumeSize { get; }


    public double[,] MatrixA { get; }
    public double[,] MatrixB { get; }


    public double[] X => ObservableVolume;
    public int SizeU => ControlVolumeSize;
    public double[] RefX => ReferenceObservationVolume;

    public double[,] A => MatrixA;
    public double[,] B => MatrixB;
}