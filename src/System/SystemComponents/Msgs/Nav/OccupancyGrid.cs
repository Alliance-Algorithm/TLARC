using System.Collections.Concurrent;
using System.Numerics;
using AllianceDM.IO;
using Rcl;

namespace AllianceDM.IO.ROS2Msgs.Nav
{
    class OccupancyGrid : TlarcMsgs
    {
        (sbyte[,] Map, float Resolution, uint Height, uint Width) data;
        Action<(sbyte[,] Map, float Resolution, uint Height, uint Width)> callback;
        ConcurrentQueue<(sbyte[,] Map, float Resolution, uint Height, uint Width)> receiveDatas = new();


        IRclPublisher<Rosidl.Messages.Nav.OccupancyGrid> publisher;
        Rcl.RosMessageBuffer nativeMsg;
        private bool publishFlag;

        void Subscript()
        {
            if (receiveDatas.Count == 0)
                return;
            while (receiveDatas.Count > 1) receiveDatas.TryDequeue(out _);

            callback(receiveDatas.Last());
        }
        void Publish()
        {
            if (publisher == null)
                return;
            publishFlag = true;
        }
        public void Subscript(string topicName, Action<(sbyte[,] Map, float Resolution, uint Height, uint Width)> callback)
        {
            this.callback = callback;
            Input += Subscript;
            IOManager.RegistrySubscription(topicName, (Rosidl.Messages.Nav.OccupancyGrid msg) =>
            {
                (sbyte[,] Map, float Resolution, uint Height, uint Width) temp = new();
                var k = msg.Data;
                temp.Map = new sbyte[msg.Info.Height, msg.Info.Width];
                temp.Resolution = msg.Info.Resolution;

                Buffer.BlockCopy(k, 0, temp.Map, 0, temp.Map.Length);

                receiveDatas.Enqueue(temp);
            });
        }
        public void RegistryPublisher(string topicName)
        {
            publisher = Ros2Def.node.CreatePublisher<Rosidl.Messages.Nav.OccupancyGrid>(topicName);
            nativeMsg = publisher.CreateBuffer();


            Task.Run(async () =>
            {
                using var timer = Ros2Def.context.CreateTimer(Ros2Def.node.Clock, TimeSpan.FromMilliseconds(value: 1));
                while (true)
                {
                    Thread.Sleep(1);
                    if (!publishFlag)
                        continue;
                    nativeMsg.AsRef<Rosidl.Messages.Nav.Path.Priv>().Poses = new(data.Map.Length);
                    var temp_map = new sbyte[data.Map.Length];
                    Buffer.BlockCopy(data.Map, 0, temp_map, 0, temp_map.Length);
                    nativeMsg.AsRef<Rosidl.Messages.Nav.OccupancyGrid.Priv>().Data.CopyFrom(temp_map);
                    nativeMsg.AsRef<Rosidl.Messages.Nav.OccupancyGrid.Priv>().Info.Height = (uint)data.Map.GetLength(0);
                    nativeMsg.AsRef<Rosidl.Messages.Nav.OccupancyGrid.Priv>().Info.Width = (uint)data.Map.GetLength(1);
                    nativeMsg.AsRef<Rosidl.Messages.Nav.OccupancyGrid.Priv>().Info.Resolution = data.Resolution;
                    publisher.Publish(nativeMsg);
                    publishFlag = false;
                    await timer.WaitOneAsync(false);
                }
            });
        }
        public void Publish((sbyte[,] Map, float Resolution, uint Height, uint Width) data)
        {
            this.data = data;
            Publish();
        }
    }
}