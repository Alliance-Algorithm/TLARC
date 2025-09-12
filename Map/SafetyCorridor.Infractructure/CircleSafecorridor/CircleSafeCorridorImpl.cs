
using Kernel.DataInterfaces;
using Kernel.DataInterfaces.Constraints;
using Kernel.DataInterfaces.Navigation;

namespace SafetyCorridor.Infractructure.CircleSafecorridor;

public readonly struct CircleSafecorridorImpl : ISafeCorridor2DData<Circle2D>
{
    public readonly required IHeader Header { get; init; }
    public readonly required int Length { get; init; }
    public readonly required Circle2D[] Corridors { get; init; }
}