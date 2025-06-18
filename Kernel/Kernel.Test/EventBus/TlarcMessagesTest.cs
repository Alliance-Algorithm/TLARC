using Kernel.Core.Messages;
using Kernel.DataInterfaces;
using Kernel.DataInterfaces.Navigation;

namespace Kernel.Test.EventBus;

internal class Map : ITlarcData;

[TestFixture]
public class TlarcMessagesTest
{
    [Test]
    public void Test()
    {
        Core.EventBus.EventBus.Instance.Publish("tlarc_message", new Map());
    }
}