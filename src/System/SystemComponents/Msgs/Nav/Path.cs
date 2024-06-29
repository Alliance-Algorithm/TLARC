using System.Collections.Concurrent;
using System.Numerics;
using AllianceDM.IO;
using Rcl;
using Rosidl.Messages.Geometry;

namespace AllianceDM.IO.ROS2Msgs.Nav
{
    class Path : TlarcMsgs
    {
        System.Numerics.Vector3[] data;
        Action<System.Numerics.Vector3[]> callback;

        static protected bool publishFlag = false;

        IRclPublisher<Rosidl.Messages.Nav.Path> publisher;
        ConcurrentQueue<System.Numerics.Vector3[]> receiveDatas = new();
        Rcl.RosMessageBuffer nativeMsg;

        void Subscript()
        {
            if (receiveDatas.Count == 0)
                return;
            while (receiveDatas.Count > 1) receiveDatas.TryDequeue(out _);
            callback(data);
        }
        void Publish()
        {
            if (publisher == null)
                return;
            publisher.Publish(nativeMsg);
            publishFlag = true;
        }
        public void Subscript(string topicName, Action<System.Numerics.Vector3[]> callback)
        {
            this.callback = callback;
            TlarcMsgs.Input += Subscript;
            IOManager.RegistrySubscription<Rosidl.Messages.Nav.Path>(topicName, (Rosidl.Messages.Nav.Path msg) =>
            {

                var k = msg.Poses;
                data = new System.Numerics.Vector3[k.Length];
                for (int i = 0; i < k.Length; i++)
                {
                    data[i] = new System.Numerics.Vector3((float)k[i].Pose.Position.X, (float)k[i].Pose.Position.Y, (float)k[i].Pose.Position.Z);
                }
            });
        }
        public void RegistryPublisher(string topicName)
        {
            publisher = Ros2Def.node.CreatePublisher<Rosidl.Messages.Nav.Path>(topicName);
            nativeMsg = publisher.CreateBuffer();


            Task.Run(async () =>
            {
                using var timer = Ros2Def.context.CreateTimer(Ros2Def.node.Clock, TimeSpan.FromMilliseconds(value: 1));
                while (true)
                {
                    Thread.Sleep(1);
                    if (!publishFlag)
                        continue;
                    nativeMsg.AsRef<Rosidl.Messages.Nav.Path.Priv>().Poses = new(data.Length);
                    for (int i = 0; i < data.Length; i++)
                    {
                        var l = new PoseStamped.Priv();
                        l.Pose.Position.X = data[i].X;
                        l.Pose.Position.Y = data[i].Y;
                        l.Pose.Position.Z = data[i].Z;
                        nativeMsg.AsRef<Rosidl.Messages.Nav.Path.Priv>().Poses.AsSpan()[i] = l;
                    }
                    publisher.Publish(nativeMsg);
                    publishFlag = false;
                    await timer.WaitOneAsync(false);
                }
            });
        }
        public void Publish(System.Numerics.Vector3[] data)
        {
            this.data = data;
            Publish();
        }
    }
}