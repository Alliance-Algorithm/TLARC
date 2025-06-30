using Kernel.DataInterfaces.Constraints;

namespace Kernel.DataInterfaces.Navigation;

public interface ISafeCorridor2D<TGeometry> : IHeader, IMap2D, ITlarcData where TGeometry : IConstraint
{
}