using AllianceDM.IO;
using Rcl;

namespace AllianceDM.IO.ROS2Msgs.Geometry
{
    class Vector3 : TlarcMsgs
    {
        public delegate void RevcAction(System.Numerics.Vector3 msg);
        System.Numerics.Vector3 data = new();
        RevcAction callback;

        static protected bool WriteLock = false;

        IRclPublisher<Rosidl.Messages.Geometry.Vector3> publisher;
        Rcl.RosMessageBuffer nativeMsg;

        void Subscript()
        {
            callback(data);
        }
        void Publish()
        {
            if (publisher == null)
                return;
            nativeMsg.AsRef<Rosidl.Messages.Geometry.Vector3.Priv>().X = data.X;
            nativeMsg.AsRef<Rosidl.Messages.Geometry.Vector3.Priv>().Y = data.Y;
            nativeMsg.AsRef<Rosidl.Messages.Geometry.Vector3.Priv>().Z = data.Z;
            publisher.Publish(nativeMsg);
            WriteLock = true;
        }
        public void Subscript(string topicName, RevcAction callback)
        {
            this.callback = callback;
            TlarcMsgs.Input += Subscript;
            IOManager.RegistrySubscription<Rosidl.Messages.Geometry.Vector3>(topicName, (Rosidl.Messages.Geometry.Vector3 msg) =>
            {

                if (TlarcMsgs.ReadLock)
                    return;
                data = new((float)msg.X, (float)msg.Y, (float)msg.Z);
            });
        }
        public void RegistetyPublisher(string topicName)
        {
            publisher = Ros2Def.node.CreatePublisher<Rosidl.Messages.Geometry.Vector3>(topicName);
            nativeMsg = publisher.CreateBuffer();
            TlarcMsgs.Output += Publish;


            Task.Run(async () =>
            {
                using var timer = Ros2Def.context.CreateTimer(Ros2Def.node.Clock, TimeSpan.FromMilliseconds(value: 1));
                while (true)
                {
                    Thread.Sleep(1);
                    if (!WriteLock)
                        continue;
                    nativeMsg.AsRef<Rosidl.Messages.Geometry.Vector3.Priv>().X = data.X;
                    nativeMsg.AsRef<Rosidl.Messages.Geometry.Vector3.Priv>().Y = data.Y;
                    nativeMsg.AsRef<Rosidl.Messages.Geometry.Vector3.Priv>().Z = data.Z;
                    publisher.Publish(nativeMsg);
                    WriteLock = false;
                    await timer.WaitOneAsync(false);
                }
            });
        }
        public void Publish(System.Numerics.Vector3 data)
        {
            this.data = data;
        }
    }
}