using System.Net.Mime;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace TlarcRosBridge.Test;

using Kernel.Core.EventBus;
using Kernel.Core.Messages;
using Rcl;
using Int32 = TlarcRosBridge.Infrastructure.Messages.Std.Int32;

[TestFixture]
public class TlarcSubscript
{
    private int all = 0;

    private void Process(in StdMessage<int> from, in IRclNode _, ref RosMessageBuffer to) =>
        to.AsRef<Int32.Priv>().Data = from.Instance;

    [SetUp]
    public void Setup()
    {
        var pub = Domain.RosBridge.Build("pub");
        var sub = Domain.RosBridge.Build("sub");
        EventBus<StdMessage<int>>.Instance.Subscribe("test_d", (StdMessage<int> x) => all +=
            x.Instance);
        sub.Subscript<Int32, StdMessage<int>>("test_c", "test_d",
            msg => StdMessage<int>.Build(msg.AsRef<Int32.Priv>().Data));
        pub.Publish<StdMessage<int>, Int32>("test_c", "test_c", Process);
    }

    [Test]
    public void Test1()
    {
        var total = 0;
        var i     = 0;
        Thread.Sleep(2);
        while (i < 20)
        {
            Thread.Sleep(1);
            i++;
            EventBus<StdMessage<int>>.Instance.Publish("test_c", StdMessage<int>.Build(i));
            total += i;
        }

        Thread.Sleep(2);

        Assert.That(all, Is.EqualTo(total));

        Environment.Exit(0);
    }
}