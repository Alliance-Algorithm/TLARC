using Kernel.Core.Messages;

namespace Kernel.Test.EventBus;

[TestFixture]
public class Communication
{
    private static readonly string Name = "communication";


    private class A
    {
        public int Total;
        private readonly int _time = Random.Shared.Next(0, 1000);

        public A()
        {
            Core.EventBus.EventBus.Instance.Subscribe(Name, Process);
            Core.EventBus.EventBus.Instance.Subscribe<StdMessage<int>>(Name, Process);
        }

        private void Process()
        {
            var date = DateTime.Now;
            Total++;
            while ((DateTime.Now - date).TotalMicroseconds < _time) ;
        }

        private void Process(StdMessage<int> data)
        {
            var date = DateTime.Now;
            Assert.That(data.Instance, Is.EqualTo(Total));
            Total++;
            while ((DateTime.Now - date).TotalMicroseconds < _time) ;
        }
    }

    [Test]
    public void Publish()
    {
        List<A> bs = [];
        for (var i = 0; i < 10; i++)
            bs.Add(new A());

        for (var i = 0; i < 100; i++)
        for (var j = 0; j < 10; j++)
        {
            var tmp = bs[j].Total;
            Core.EventBus.EventBus.Instance.Publish(Name);
            Assert.That(bs[j].Total, Is.EqualTo(tmp + 1));
            Core.EventBus.EventBus.Instance.Publish(Name, StdMessage<int>.Build(bs[j].Total));
        }
    }
}