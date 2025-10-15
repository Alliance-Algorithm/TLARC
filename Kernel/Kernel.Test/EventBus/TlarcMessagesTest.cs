using Kernel.Core.Messages;
using Kernel.Contract;
using Kernel.Contract.Navigation;

namespace Kernel.Test.EventBus;

internal class Map : ITlarcData;

[TestFixture]
public class TlarcMessagesTest
{
    [Test]
    public void Test() => Core.EventBus.EventBus<ITlarcData>.Instance.Publish("tlarc_message", new Map());
}