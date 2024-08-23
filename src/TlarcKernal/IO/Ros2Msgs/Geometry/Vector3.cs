using System.Collections.Concurrent;
using Tlarc.IO;
using Rcl;

namespace Tlarc.IO.ROS2Msgs.Geometry
{
    class Vector3(IOManager io)
    {
        System.Numerics.Vector3 data = new();
        Action<System.Numerics.Vector3> callback;
        ConcurrentQueue<System.Numerics.Vector3> receiveData = new();
        IOManager _ioManager = io;


        IRclPublisher<Rosidl.Messages.Geometry.Vector3> publisher;
        RosMessageBuffer nativeMsg;
        private bool publishFlag;

        void Subscript()
        {
            if (receiveData.Count == 0)
                return;
            while (receiveData.Count > 1) receiveData.TryDequeue(out _);
            callback(receiveData.First());
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
            _ioManager.TlarcRosMsgs.Input += Subscript;
            _ioManager.RegistrySubscription(topicName, (Rosidl.Messages.Geometry.Vector3 msg) =>
            {
                receiveData.Enqueue(new((float)msg.X, (float)msg.Y, (float)msg.Z));
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