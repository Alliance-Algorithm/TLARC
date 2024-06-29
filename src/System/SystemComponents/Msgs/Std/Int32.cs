using System.Collections.Concurrent;
using AllianceDM.IO;
using Rcl;

namespace AllianceDM.IO.ROS2Msgs.Std
{
    class Int32 : TlarcMsgs
    {
        int data = 0;
        Action<int> callback;

        static protected bool publishFlag = false;

        IRclPublisher<Rosidl.Messages.Std.Int32> publisher;
        ConcurrentQueue<int> receiveDatas = new();
        Rcl.RosMessageBuffer nativeMsg;

        void Subscript()
        {
            if (receiveDatas.Count == 0)
                return;
            while (receiveDatas.Count > 1) receiveDatas.TryDequeue(out _);
            callback(receiveDatas.Last());
        }
        void Publish()
        {
            if (publisher == null)
                return;
            nativeMsg.AsRef<Rosidl.Messages.Std.Int32.Priv>().Data = data;
            publisher.Publish(nativeMsg);
            publishFlag = true;
        }
        public void Subscript(string topicName, Action<int> callback)
        {
            this.callback = callback;
            TlarcMsgs.Input += Subscript;
            IOManager.RegistrySubscription<Rosidl.Messages.Std.Int32>(topicName, (Rosidl.Messages.Std.Int32 msg) =>
            {
                data = msg.Data;
            });
        }
        public void RegistetyPublisher(string topicName)
        {
            publisher = Ros2Def.node.CreatePublisher<Rosidl.Messages.Std.Int32>(topicName);
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
                    nativeMsg.AsRef<Rosidl.Messages.Std.Int32.Priv>().Data = (int)data;
                    publisher.Publish(nativeMsg);
                    publishFlag = false;
                }
            });
        }
        public void Publish(int data)
        {
            this.data = data;
            Publish();
        }
    }
}