using Kernel.Core.Messages;

namespace Kernel.Test.EventBus;

[TestFixture]
public class StressTest
{
    [Test] // 标识测试方法
    public void IsDataAllRight()
    {
        var a = "a";
        var b = "b";
        var c = "c";
        var da = 1;
        var db = 1.0f;
        var dc = 'a';

        var totalA = 0;
        var totalB = 0;
        var totalC = 0;

        for (var i = 0; i < 1000; i++)
        {
            Core.EventBus.EventBus.Instance.Subscribe(a, (StdMessage<int> data) =>
            {
                Assert.That(data.Instance, Is.EqualTo(da));
                Interlocked.Add(ref totalA, 1);
            });
            Core.EventBus.EventBus.Instance.Subscribe(b, (StdMessage<float> data) =>
            {
                Assert.That(data.Instance, Is.EqualTo(db));
                Interlocked.Add(ref totalB, 1);
            });
            Core.EventBus.EventBus.Instance.Subscribe(c, (StdMessage<char> data) =>
            {
                Assert.That(data.Instance, Is.EqualTo(dc));
                Interlocked.Add(ref totalC, 1);
            });
        }

        const int max = 10000;

        for (var i = 0; i < max; i++)
        {
            Core.EventBus.EventBus.Instance.Publish(a, StdMessage<int>.Build(da));
            Core.EventBus.EventBus.Instance.Publish(b, StdMessage<float>.Build(db));
            Core.EventBus.EventBus.Instance.Publish(b, StdMessage<int>.Build(da));
            Core.EventBus.EventBus.Instance.Publish(c, StdMessage<char>.Build(dc));
            da++;
            db++;
        }

        Assert.That(totalA, Is.EqualTo(max * 1000));
    }
}