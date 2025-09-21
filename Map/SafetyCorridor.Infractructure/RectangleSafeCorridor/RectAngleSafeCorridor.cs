
using Kernel.DataInterfaces;
using Kernel.DataInterfaces.Constraints;
using Kernel.DataInterfaces.Navigation;
using Kernel.DataInterfaces.Visualization;

namespace SafetyCorridor.Infractructure.RectangleSafeCorridor;

public readonly struct RectangleSafecorridorImpl : ISafeCorridor2DData<IRectangle>
{
    public readonly required IHeader Header { get; init; }
    public readonly required int Length { get; init; }
    public readonly required IRectangle[] Corridors { get; init; }
}