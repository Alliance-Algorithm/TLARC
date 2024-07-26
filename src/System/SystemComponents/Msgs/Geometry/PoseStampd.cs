using System.Collections.Concurrent;
using System.Numerics;
using AllianceDM.IO;
using Rcl;

namespace AllianceDM.IO.ROS2Msgs.Geometry
{
    class PoseStampd : TlarcMsgs
    {
        (Vector2 pos, float Theta) data = new();
        Action<(Vector2 pos, float Theta)> callback;
        ConcurrentQueue<(Vector2 pos, float Theta)> receiveDatas = new();

        IRclPublisher<Rosidl.Messages.Geometry.PoseStamped> publisher;
        RosMessageBuffer nativeMsg;
        private bool publishFlag;

        void Subscript()
        {
            if (receiveDatas == null || receiveDatas.Count == 0)
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
        public void Subscript(string topicName, Action<(Vector2 pos, float Theta)> callback)
        {
            this.callback = callback;
            Input += Subscript;
            IOManager.RegistrySubscription(topicName, (Rosidl.Messages.Geometry.PoseStamped msg) =>
            {
                var q = msg.Pose.Orientation;
                double siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
                double cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
                var angle = Math.PI + Math.Atan2(siny_cosp, cosy_cosp);

                receiveDatas.Enqueue((new((float)msg.Pose.Position.X, (float)msg.Pose.Position.Y), (float)angle));
            });
        }
        public void RegistryPublisher(string topicName)
        {
            publisher = Ros2Def.node.CreatePublisher<Rosidl.Messages.Geometry.PoseStamped>(topicName);
            nativeMsg = publisher.CreateBuffer();


            Task.Run(async () =>
            {
                using var timer = Ros2Def.context.CreateTimer(Ros2Def.node.Clock, TimeSpan.FromMilliseconds(1));
                while (true)
                {
                    await timer.WaitOneAsync(false);
                    if (!publishFlag)
                        continue;
                    nativeMsg.AsRef<Rosidl.Messages.Geometry.PoseStamped.Priv>().Pose.Position.X = data.pos.X;
                    nativeMsg.AsRef<Rosidl.Messages.Geometry.PoseStamped.Priv>().Pose.Position.Y = data.pos.Y;
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