using System.Numerics;
using AllianceDM.IO;
using Rcl;

namespace AllianceDM.IO.ROS2Msgs.Geometry
{
    class Pose2D : TlarcMsgs
    {
        public delegate void RevcAction((Vector2 pos, float Theta) msg);
        (Vector2 pos, float Theta) data = new();
        RevcAction callback;

        static protected bool WriteLock = false;

        IRclPublisher<Rosidl.Messages.Geometry.Pose2D> publisher;
        Rcl.RosMessageBuffer nativeMsg;

        void Subscript()
        {
            callback(data);
        }
        void Publish()
        {
            if (publisher == null)
                return;
            nativeMsg.AsRef<Rosidl.Messages.Geometry.Pose2D.Priv>().X = data.pos.X;
            nativeMsg.AsRef<Rosidl.Messages.Geometry.Pose2D.Priv>().Y = data.pos.Y;
            nativeMsg.AsRef<Rosidl.Messages.Geometry.Pose2D.Priv>().Theta = data.Theta;
            publisher.Publish(nativeMsg);
            WriteLock = true;
        }
        public void Subscript(string topicName, RevcAction callback)
        {
            this.callback = callback;
            TlarcMsgs.Input += Subscript;
            IOManager.RegistrySubscription<Rosidl.Messages.Geometry.Pose2D>(topicName, (Rosidl.Messages.Geometry.Pose2D msg) =>
            {

                if (TlarcMsgs.ReadLock)
                    return;
                data = (new((float)msg.X, (float)msg.Y), (float)msg.Theta);
            });
        }
        public void RegistetyPublisher(string topicName)
        {
            publisher = Ros2Def.node.CreatePublisher<Rosidl.Messages.Geometry.Pose2D>(topicName);
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
                    nativeMsg.AsRef<Rosidl.Messages.Geometry.Pose2D.Priv>().X = data.pos.X;
                    nativeMsg.AsRef<Rosidl.Messages.Geometry.Pose2D.Priv>().Y = data.pos.Y;
                    nativeMsg.AsRef<Rosidl.Messages.Geometry.Pose2D.Priv>().Theta = data.Theta;
                    publisher.Publish(nativeMsg);
                    WriteLock = false;
                    await timer.WaitOneAsync(false);
                }
            });
        }
        public void Publish((Vector2 pos, float Theta) data)
        {
            this.data = data;
        }
    }
}