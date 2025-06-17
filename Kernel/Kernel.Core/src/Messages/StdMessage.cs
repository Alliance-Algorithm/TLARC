using Kernel.Core.EventBus;
using Kernel.DataInterfaces;

namespace Kernel.Core.Messages;

public class StdMessage<TInterface> : ITlarcData where TInterface : struct
{
    private StdMessage() { }
    public required TInterface Instance;

    public static StdMessage<TInterface> Build(TInterface instance)
    {
        return new StdMessage<TInterface> { Instance = instance };
    }
}
