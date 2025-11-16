using System.Numerics;

namespace Kernel.Contract.Navigation;

public interface IEnumableGridMap : ITlarcData
{
    [Flags]
    public enum StateEnum
    {
        Occu    = 1,
        Free    = 2,
        Unknow  = 4,
    }
    public void All(StateEnum state,Action<Vector2,StateEnum> callback, bool paralized = false); 
}
