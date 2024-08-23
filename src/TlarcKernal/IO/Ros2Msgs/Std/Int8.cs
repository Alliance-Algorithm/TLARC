using System.Collections.Concurrent;
using Tlarc.IO;
using Rcl;

namespace Tlarc.IO.ROS2Msgs.Std
{
    class Int8(IOManager io)
    {
        sbyte data = 0;
        Action<sbyte> callback;

        static protected bool publishFlag = false;

        IRclPublisher<Rosidl.Messages.Std.Int8> publisher;
        ConcurrentQueue<sbyte> receiveData = new();
        Rcl.RosMessageBuffer nativeMsg;
        IOManager _ioManager = io;

        void Subscript()
        {
            if (receiveData.Count == 0)
                return;
            while (receiveData.Count > 1) receiveData.TryDequeue(out _);
            callback(receiveData.Last());
        }
        void Publish()
        {
            if (publisher == null)
                return;
            nativeMsg.AsRef<Rosidl.Messages.Std.Int8.Priv>().Data = data;
            publisher.Publish(nativeMsg);
            publishFlag = true;
        }
        public void Subscript(string topicName, Action<sbyte> callback)
        {
            this.callback = callback;
            _ioManager.TlarcRosMsgs.Input += Subscript;
            _ioManager.RegistrySubscription<Rosidl.Messages.Std.Int8>(topicName, (Rosidl.Messages.Std.Int8 msg) =>
            {
                receiveData.Enqueue(msg.Data);
            });
        }
        public void RegistryPublisher(string topicName)
        {
            publisher = Ros2Def.node.CreatePublisher<Rosidl.Messages.Std.Int8>(topicName);
            nativeMsg = publisher.CreateBuffer();
            // TlarcMsgs.Output += Publish;

            Task.Run(async () =>
            {
                using var timer = Ros2Def.context.CreateTimer(Ros2Def.node.Clock, TimeSpan.FromMilliseconds(value: 1));
                while (true)
                {
                    await timer.WaitOneAsync(false);
                    if (!publishFlag)
                        continue;
                    nativeMsg.AsRef<Rosidl.Messages.Std.Int8.Priv>().Data = data;
                    publisher.Publish(nativeMsg);
                    publishFlag = false;
                }
            });
        }
        public void Publish(sbyte data)
        {
            this.data = data;
            Publish();
        }
    }
}