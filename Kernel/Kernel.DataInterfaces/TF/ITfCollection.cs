namespace Kernel.DataInterfaces.Tf;

public interface ITfCollection : ITlarcData
{
    public ITransformStamped[] TransformStampeds { get; }
}