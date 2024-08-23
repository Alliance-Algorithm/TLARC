using System.Collections.Concurrent;
using System.Numerics;
using Tlarc.IO;
using Rcl;

namespace Tlarc.IO.ROS2Msgs.Nav
{
    class OccupancyGrid(IOManager io)
    {
        (sbyte[,] Map, float Resolution, uint Height, uint Width) data;
        Action<(sbyte[,] Map, float Resolution, uint Height, uint Width)> callback;
        ConcurrentQueue<(sbyte[,] Map, float Resolution, uint Height, uint Width)> receiveData = new();

        IOManager _ioManager = io;
        IRclPublisher<Rosidl.Messages.Nav.OccupancyGrid> publisher;
        Rcl.RosMessageBuffer nativeMsg;
        private bool publishFlag;

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
        public void Subscript(string topicName, Action<(sbyte[,] Map, float Resolution, uint Height, uint Width)> callback)
        {
            this.callback = callback;
            _ioManager.TlarcRosMsgs.Input += Subscript;
            _ioManager.RegistrySubscription(topicName, (Rosidl.Messages.Nav.OccupancyGrid msg) =>
            {
                (sbyte[,] Map, float Resolution, uint Height, uint Width) temp = new();
                var k = msg.Data;
                temp.Map = new sbyte[msg.Info.Height, msg.Info.Width];
                temp.Resolution = msg.Info.Resolution;

                Buffer.BlockCopy(k, 0, temp.Map, 0, temp.Map.Length);

                receiveData.Enqueue(temp);
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
                    await timer.WaitOneAsync(false);
                    if (!publishFlag)
                        continue;
                    var temp_map = new sbyte[data.Map.Length];
                    Buffer.BlockCopy(data.Map, 0, temp_map, 0, temp_map.Length);
                    nativeMsg.AsRef<Rosidl.Messages.Nav.OccupancyGrid.Priv>().Data.CopyFrom(temp_map);
                    nativeMsg.AsRef<Rosidl.Messages.Nav.OccupancyGrid.Priv>().Info.Height = (uint)data.Map.GetLength(0);
                    nativeMsg.AsRef<Rosidl.Messages.Nav.OccupancyGrid.Priv>().Info.Width = (uint)data.Map.GetLength(1);
                    nativeMsg.AsRef<Rosidl.Messages.Nav.OccupancyGrid.Priv>().Info.Resolution = data.Resolution;
                    nativeMsg.AsRef<Rosidl.Messages.Nav.OccupancyGrid.Priv>().Header.FrameId.CopyFrom("tlarc");
                    nativeMsg.AsRef<Rosidl.Messages.Nav.OccupancyGrid.Priv>().Info.Origin.Position.X = -14 - 7.5;
                    nativeMsg.AsRef<Rosidl.Messages.Nav.OccupancyGrid.Priv>().Info.Origin.Position.Y = -7.5;
                    nativeMsg.AsRef<Rosidl.Messages.Nav.OccupancyGrid.Priv>().Info.Origin.Orientation.W = -1;
                    publisher.Publish(nativeMsg);
                    nativeMsg.Dispose();
                    nativeMsg = publisher.CreateBuffer();
                    publishFlag = false;
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