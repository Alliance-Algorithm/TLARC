using System.Collections.Concurrent;
using AllianceDM.IO;
using Rcl;

namespace AllianceDM.IO.ROS2Msgs.Std
{
    class Int8 : TlarcMsgs
    {
        sbyte data = 0;
        Action<sbyte> callback;

        static protected bool publishFlag = false;

        IRclPublisher<Rosidl.Messages.Std.Int8> publisher;
        ConcurrentQueue<sbyte> receiveData = new();
        Rcl.RosMessageBuffer nativeMsg;

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
            TlarcMsgs.Input += Subscript;
            IOManager.RegistrySubscription<Rosidl.Messages.Std.Int8>(topicName, (Rosidl.Messages.Std.Int8 msg) =>
            {
                data = msg.Data;
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