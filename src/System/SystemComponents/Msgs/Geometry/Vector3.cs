using System.Collections.Concurrent;
using AllianceDM.IO;
using Rcl;

namespace AllianceDM.IO.ROS2Msgs.Geometry
{
    class Vector3 : TlarcMsgs
    {
        System.Numerics.Vector3 data = new();
        Action<System.Numerics.Vector3> callback;
        ConcurrentQueue<System.Numerics.Vector3> receiveDatas = new();


        IRclPublisher<Rosidl.Messages.Geometry.Vector3> publisher;
        RosMessageBuffer nativeMsg;
        private bool publishFlag;

        void Subscript()
        {
            if (receiveDatas.Count == 0)
                return;
            while (receiveDatas.Count > 1) receiveDatas.TryDequeue(out _);
            callback(receiveDatas.First());
        }
        void Publish()
        {
            if (publisher == null)
                return;
            publishFlag = true;
        }
        public void Subscript(string topicName, Action<System.Numerics.Vector3> callback)
        {
            this.callback = callback;
            TlarcMsgs.Input += Subscript;
            IOManager.RegistrySubscription(topicName, (Rosidl.Messages.Geometry.Vector3 msg) =>
            {
                receiveDatas.Enqueue(new((float)msg.X, (float)msg.Y, (float)msg.Z));
            });
        }
        public void RegistryPublisher(string topicName)
        {
            publisher = Ros2Def.node.CreatePublisher<Rosidl.Messages.Geometry.Vector3>(topicName);
            nativeMsg = publisher.CreateBuffer();


            Task.Run(async () =>
            {
                using var timer = Ros2Def.context.CreateTimer(Ros2Def.node.Clock, TimeSpan.FromMilliseconds(value: 1));
                while (true)
                {
                    await timer.WaitOneAsync(false);
                    if (!publishFlag)
                        continue;
                    nativeMsg.AsRef<Rosidl.Messages.Geometry.Vector3.Priv>().X = data.X;
                    nativeMsg.AsRef<Rosidl.Messages.Geometry.Vector3.Priv>().Y = data.Y;
                    nativeMsg.AsRef<Rosidl.Messages.Geometry.Vector3.Priv>().Z = data.Z;
                    publisher.Publish(nativeMsg);
                    publishFlag = false;
                }
            });
        }
        public void Publish(System.Numerics.Vector3 data)
        {
            this.data = data;
            Publish();
        }
    }
}