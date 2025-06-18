using Kernel.Core.EventBus;
using Kernel.Core.Messages;
using Kernel.DataInterfaces;
using Rcl;
using Rosidl.Runtime;

namespace TlarcRosBridge.Domain;

public class RosBridge
{
    private List<PublisherRecord> _publisherRecords = [];

    private class PublisherRecord(IRclPublisher publisher, RosMessageBuffer buffer)
    {
        public readonly IRclPublisher Publisher = publisher;
        public RosMessageBuffer Buffer = buffer;
    }

    private static readonly Lazy<RclContext> Context = new(() => new RclContext());
    public required string NodeName { private get; init; }
    public required IRclNode Node { private get; init; }

    private RosBridge()
    {
    }

    public void Subscript<TMessage, TTlarcData>(string                             rosTopicName,
                                                string                             tlarcEventName,
                                                Func<RosMessageBuffer, TTlarcData> dataFunc)
        where TMessage : IMessage where TTlarcData : ITlarcData
    {
        Task.Run(async () =>
        {
            using var sub = Node.CreateNativeSubscription<TMessage>(rosTopicName);

            await foreach (var msg in sub.ReadAllAsync())
                using (msg)
                {
                    EventBus.Instance.Publish(tlarcEventName, dataFunc(msg));
                }
        });
        Thread.Sleep(1);
    }

    public void Publish<TTlarcData, TMessage>(string                                            tlarcEventName,
                                              string                                            rosTopicName,
                                              RefAction<TTlarcData, IRclNode, RosMessageBuffer> dataFunc)
        where TMessage : IMessage where TTlarcData : ITlarcData
    {
        var pub = Node.CreatePublisher<TMessage>(rosTopicName);
        var rec = new PublisherRecord(pub, pub.CreateBuffer());
        _publisherRecords.Add(rec);
        EventBus.Instance.Subscribe(tlarcEventName, (TTlarcData data) =>
            {
                var tmp = Node;
                dataFunc(in data, in tmp, ref rec.Buffer);
                rec.Publisher.Publish(rec.Buffer);
            }
        );

        Thread.Sleep(1);
    }


    public static RosBridge Build(string nodeName)
    {
        return new RosBridge
        {
            NodeName = nodeName,
            Node = Context.Value.CreateNode(nodeName)
        };
    }
}