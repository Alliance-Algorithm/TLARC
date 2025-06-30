using System.Numerics;
using Kernel.DataInterfaces.Geometry;

namespace Kernel.DataInterfaces.Tf;

public interface ITransformStamped : ITlarcData
{
    public IHeader Header { get; }
    public string ParentFrameId { get; }
    public IPose Pose { get; }
}