using System.Collections.Concurrent;
using System.Numerics;
using Tlarc.IO;
using Rcl;

namespace Tlarc.IO.ROS2Msgs.Geometry
{
    class Pose2D(IOManager io)
    {
        (Vector2 pos, float Theta) data = new();
        Action<(Vector2 pos, float Theta)> callback;
        ConcurrentQueue<(Vector2 pos, float Theta)> receiveData = new();

        IRclPublisher<Rosidl.Messages.Geometry.Pose2D> publisher;
        RosMessageBuffer nativeMsg;
        IOManager _ioManager = io;
        private bool publishFlag;

        void Subscript()
        {
            if (receiveData == null || receiveData.Count == 0)
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
        public void Subscript(string topicName, Action<(Vector2 pos, float Theta)> callback)
        {
            this.callback = callback;
            _ioManager.TlarcRosMsgs.Input += Subscript;
            _ioManager.RegistrySubscription(topicName, (Rosidl.Messages.Geometry.Pose2D msg) =>
            {
                receiveData.Enqueue((new((float)msg.X, (float)msg.Y), (float)msg.Theta));
            });
        }
        public void RegistryPublisher(string topicName)
        {
            publisher = Ros2Def.node.CreatePublisher<Rosidl.Messages.Geometry.Pose2D>(topicName);
            nativeMsg = publisher.CreateBuffer();


            Task.Run(async () =>
            {
                using var timer = Ros2Def.context.CreateTimer(Ros2Def.node.Clock, TimeSpan.FromMilliseconds(1));
                while (true)
                {
                    await timer.WaitOneAsync(false);
                    if (!publishFlag)
                        continue;
                    nativeMsg.AsRef<Rosidl.Messages.Geometry.Pose2D.Priv>().X = data.pos.X;
                    nativeMsg.AsRef<Rosidl.Messages.Geometry.Pose2D.Priv>().Y = data.pos.Y;
                    nativeMsg.AsRef<Rosidl.Messages.Geometry.Pose2D.Priv>().Theta = data.Theta;
                    publisher.Publish(nativeMsg);
                    publishFlag = false;
                }
            });
        }
        public void Publish((Vector2 pos, float Theta) data)
        {
            this.data = data;
            Publish();
        }
    }
}