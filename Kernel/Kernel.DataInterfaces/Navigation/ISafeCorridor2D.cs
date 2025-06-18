using Kernel.DataInterfaces.Constraints;

namespace Kernel.DataInterfaces.Navigation;

public interface ISafeCorridor2D<TGeometry> : ITransform, IMap2D, ITlarcData where TGeometry : IConstraint
{
}