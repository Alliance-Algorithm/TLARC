using System.Numerics;
using AllianceDM.IO;
using Rcl;
using Rosidl.Messages.Geometry;

namespace AllianceDM.IO.ROS2Msgs.Nav
{
    class Path : TlarcMsgs
    {
        public delegate void RevcAction(System.Numerics.Vector3[] msg);
        System.Numerics.Vector3[] data;
        RevcAction callback;

        static protected bool WriteLock = false;

        IRclPublisher<Rosidl.Messages.Nav.Path> publisher;
        Rcl.RosMessageBuffer nativeMsg;

        void Subscript()
        {
            if (data == null)
                return;
            callback(data);
        }
        void Publish()
        {
            if (publisher == null)
                return;
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
            WriteLock = true;
        }
        public void Subscript(string topicName, RevcAction callback)
        {
            this.callback = callback;
            TlarcMsgs.Input += Subscript;
            IOManager.RegistrySubscription<Rosidl.Messages.Nav.Path>(topicName, (Rosidl.Messages.Nav.Path msg) =>
            {

                if (TlarcMsgs.ReadLock)
                    return;
                var k = msg.Poses;
                data = new System.Numerics.Vector3[k.Length];
                for (int i = 0; i < k.Length; i++)
                {
                    data[i] = new System.Numerics.Vector3((float)k[i].Pose.Position.X, (float)k[i].Pose.Position.Y, (float)k[i].Pose.Position.Z);
                }
            });
        }
        public void RegistetyPublisher(string topicName)
        {
            publisher = Ros2Def.node.CreatePublisher<Rosidl.Messages.Nav.Path>(topicName);
            nativeMsg = publisher.CreateBuffer();
            // TlarcMsgs.Output += Publish;


            Task.Run(async () =>
            {
                using var timer = Ros2Def.context.CreateTimer(Ros2Def.node.Clock, TimeSpan.FromMilliseconds(value: 1));
                while (true)
                {
                    await timer.WaitOneAsync(false);
                    if (!WriteLock)
                        continue;
                    publisher.Publish(nativeMsg);
                    WriteLock = false;
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