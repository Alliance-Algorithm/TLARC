using Kernel.Core.EventBus;
using Kernel.DataInterfaces;

namespace Kernel.Core.Messages;

public class StdMessage<TInterface> : ITlarcData where TInterface : struct
{
    private StdMessage() { }
    public required TInterface Instance;

    public static StdMessage<TInterface> Build(TInterface instance) => new() { Instance = instance };
}

public class StringMessage : ITlarcData
{
    private StringMessage() { }
    public required string Instance;

    public static StringMessage Build(string instance) => new() { Instance = instance };
}