using System.ComponentModel;
using Kernel.DataInterfaces.Constraints;

namespace Kernel.DataInterfaces.Navigation;

public interface ISafeCorridor2DData<TGeometry> : ITlarcData where TGeometry : IConstraint
{
    IHeader Header { get; }
    int Length { get; }
    TGeometry[] Corridors { get; }
}
