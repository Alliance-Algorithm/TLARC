
using TlarcKernel.IO;
using TlarcKernel.IO.ROS2Msgs.Std;

namespace Engineer.Arm;

class Joint
{
    public Joint(string JointName, IOManager io)
    {
        jointName = $"/{JointName}/value";
        float64 = new(io);
        float64.Subscript(jointName, x => Value = x);
    }
    public string jointName { get; init; }

    public double Value { get; private set; }
    private Float64 float64;

}

static class JointExt
{
    public static double[] GetValueArray(this Joint[] joints)
    {
        return (double[])joints.Select(joint => joint.Value);
    }
}