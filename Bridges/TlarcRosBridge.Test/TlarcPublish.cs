namespace TlarcRosBridge.Test;

using Kernel.Core.EventBus;
using Kernel.Core.Messages;
using Rcl;
using Int32 = TlarcRosBridge.Infrastructure.Messages.Std.Int32;

[TestFixture]
public class TlarcPublish
{
    private void Process(in StdMessage<int> from, in IRclNode _, ref RosMessageBuffer to)
    {
        to.AsRef<Int32.Priv>().Data = from.Instance;
    }

    [SetUp]
    public void Setup()
    {
        var ros = Domain.RosBridge.Build("pub1");
        ros.Publish<StdMessage<int>, Int32>("test_a", "test_a", Process);
    }

    [Test]
    public void Test1()
    {
        var i = 0;

        while (i < 20) EventBus.Instance.Publish("test_a", StdMessage<int>.Build(i++));
        Thread.Sleep(100);
    }
}