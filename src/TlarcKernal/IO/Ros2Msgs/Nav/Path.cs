using System.Collections.Concurrent;
using System.Numerics;
using Tlarc.IO;
using Rcl;
using Rosidl.Messages.Geometry;

namespace Tlarc.IO.ROS2Msgs.Nav
{
    class Path(IOManager io)
    {
        System.Numerics.Vector3[] data;
        Action<System.Numerics.Vector3[]> callback;

        private bool publishFlag;

        IRclPublisher<Rosidl.Messages.Nav.Path> publisher;
        ConcurrentQueue<System.Numerics.Vector3[]> receiveData = new();
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
            publishFlag = true;
        }
        public void Subscript(string topicName, Action<System.Numerics.Vector3[]> callback)
        {
            this.callback = callback;
            _ioManager.TlarcRosMsgs.Input += Subscript;
            _ioManager.RegistrySubscription<Rosidl.Messages.Nav.Path>(topicName, (Rosidl.Messages.Nav.Path msg) =>
            {

                var k = msg.Poses;
                var data = new System.Numerics.Vector3[k.Length];
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
                    await timer.WaitOneAsync(false);
                    if (!publishFlag)
                        continue;
                    nativeMsg.AsRef<Rosidl.Messages.Nav.Path.Priv>().Poses = new(data.Length);
                    nativeMsg.AsRef<Rosidl.Messages.Nav.Path.Priv>().Header.FrameId.CopyFrom("tlarc");
                    for (int i = 0; i < data.Length; i++)
                    {
                        var l = new PoseStamped.Priv();
                        l.Pose.Position.X = -7.5 - data[i].X;
                        l.Pose.Position.Y = -data[i].Y;
                        l.Pose.Position.Z = data[i].Z;
                        l.Pose.Orientation.W = 1;
                        l.Header.FrameId.CopyFrom("tlarc");
                        nativeMsg.AsRef<Rosidl.Messages.Nav.Path.Priv>().Poses.AsSpan()[i] = l;
                    }
                    publisher.Publish(nativeMsg);
                    publishFlag = false;
                }
            });
        }
        public void Publish(System.Numerics.Vector3[] data)
        {
            this.data = data;
            Publish();
        }
        public void Publish(System.Numerics.Vector2[] data)
        {
            this.data = new System.Numerics.Vector3[data.Length];
            for (int i = 0; i < this.data.Length; i++)
                this.data[i] = new(data[i], 0);

            Publish();
        }
    }
}