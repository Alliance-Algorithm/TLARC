using System.ComponentModel;
using Kernel.Contract.Constraints;

namespace Kernel.Contract.Navigation;

public struct SafeCorridor2DData<TGeometry> : ITlarcData where TGeometry : IConstraint
{
    public Header       Header;
    public int          Length;
    public TGeometry[]  Corridors; 
}
