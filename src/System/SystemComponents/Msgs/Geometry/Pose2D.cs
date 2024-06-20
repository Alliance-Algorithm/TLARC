using System.Collections.Concurrent;
using System.Numerics;
using AllianceDM.IO;
using Rcl;

namespace AllianceDM.IO.ROS2Msgs.Geometry
{
    class Pose2D : TlarcMsgs
    {
        (Vector2 pos, float Theta) data = new();
        Action<(Vector2 pos, float Theta)> callback;
        ConcurrentQueue<(Vector2 pos, float Theta)> recieveDatas = new();

        IRclPublisher<Rosidl.Messages.Geometry.Pose2D> publisher;
        RosMessageBuffer nativeMsg;
        private bool publishFlag;

        void Subscript()
        {
            if (recieveDatas.Count == 0)
                return;
            recieveDatas = recieveDatas.TakeLast(1) as ConcurrentQueue<(Vector2 pos, float Theta)>;
            callback(recieveDatas.First());
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
            IOManager.RegistrySubscription(topicName, (Rosidl.Messages.Geometry.Pose2D msg) =>
            {
                recieveDatas.Enqueue((new((float)msg.X, (float)msg.Y), (float)msg.Theta));
            });
        }
        public void RegistetyPublisher(string topicName)
        {
            publisher = Ros2Def.node.CreatePublisher<Rosidl.Messages.Geometry.Pose2D>(topicName);
            nativeMsg = publisher.CreateBuffer();


            Task.Run(async () =>
            {
                using var timer = Ros2Def.context.CreateTimer(Ros2Def.node.Clock, TimeSpan.FromMilliseconds(1));
                while (true)
                {
                    Thread.Sleep(1);
                    if (!publishFlag)
                        continue;
                    nativeMsg.AsRef<Rosidl.Messages.Geometry.Pose2D.Priv>().X = data.pos.X;
                    nativeMsg.AsRef<Rosidl.Messages.Geometry.Pose2D.Priv>().Y = data.pos.Y;
                    nativeMsg.AsRef<Rosidl.Messages.Geometry.Pose2D.Priv>().Theta = data.Theta;
                    publisher.Publish(nativeMsg);
                    publishFlag = false;
                    await timer.WaitOneAsync(false);
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