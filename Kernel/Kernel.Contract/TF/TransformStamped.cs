using System.Numerics;
using Kernel.Contract.Geometry;

namespace Kernel.Contract.Tf;

public struct TransformStamped : ITlarcData
{
    public Header   Header;
    public string   ParentFrameId;
    public Pose    Pose;
}