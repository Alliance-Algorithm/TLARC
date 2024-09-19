namespace TrajectoryTracer.CarModels;

class CarModel : ICarModel
{
    public double[] ControlVolume => throw new NotImplementedException();

    public double[] ObservableVolume => throw new NotImplementedException();

    public double[] ReferenceObservationVolume => throw new NotImplementedException();

    public double[,] MatrixA => throw new NotImplementedException();

    public double[,] MatrixB => throw new NotImplementedException();
}