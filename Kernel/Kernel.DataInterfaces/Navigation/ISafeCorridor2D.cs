using Kernel.DataInterfaces.Constraints;

namespace Kernel.DataInterfaces.Navigation;

public interface ISafeCorridor2D<TGeometry> : IHeader, ITlarcData where TGeometry : IConstraint
{
    IHeader Header { get; }
    int Length { get; }
    TGeometry[] Corridors { get; }
}