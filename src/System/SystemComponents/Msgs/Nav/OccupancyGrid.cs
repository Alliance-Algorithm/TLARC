using System.Numerics;
using AllianceDM.IO;
using Rcl;
using Rosidl.Messages.Geometry;
using Rosidl.Messages.Std;

namespace AllianceDM.IO.ROS2Msgs.Nav
{
    class OccupancyGrid : TlarcMsgs
    {
        public delegate void RevcAction((sbyte[,] Map, float Resolution, uint Height, uint Width) msg);
        (sbyte[,] Map, float Resolution, uint Height, uint Width) data;
        RevcAction callback;

        static protected bool WriteLock = false;

        IRclPublisher<Rosidl.Messages.Nav.OccupancyGrid> publisher;
        Rcl.RosMessageBuffer nativeMsg;

        void Subscript()
        {
            if (data.Map == null)
                return;
            callback(data);
        }
        void Publish()
        {
            if (publisher == null)
                return;
            nativeMsg.AsRef<Rosidl.Messages.Nav.Path.Priv>().Poses = new(data.Map.Length);
            var temp_map = new sbyte[data.Map.Length];
            Buffer.BlockCopy(data.Map, 0, temp_map, 0, temp_map.Length);
            nativeMsg.AsRef<Rosidl.Messages.Nav.OccupancyGrid.Priv>().Data.CopyFrom(temp_map);
            nativeMsg.AsRef<Rosidl.Messages.Nav.OccupancyGrid.Priv>().Info.Height = (uint)data.Map.GetLength(0);
            nativeMsg.AsRef<Rosidl.Messages.Nav.OccupancyGrid.Priv>().Info.Width = (uint)data.Map.GetLength(1);
            nativeMsg.AsRef<Rosidl.Messages.Nav.OccupancyGrid.Priv>().Info.Resolution = data.Resolution;

            publisher.Publish(nativeMsg);
            WriteLock = true;
        }
        public void Subscript(string topicName, RevcAction callback)
        {
            this.callback = callback;
            TlarcMsgs.Input += Subscript;
            IOManager.RegistrySubscription<Rosidl.Messages.Nav.OccupancyGrid>(topicName, (Rosidl.Messages.Nav.OccupancyGrid msg) =>
            {

                if (TlarcMsgs.ReadLock)
                    return;
                var k = msg.Data;
                data.Map = new sbyte[msg.Info.Height, msg.Info.Width];
                data.Resolution = msg.Info.Resolution;
                data.Height = msg.Info.Height;
                data.Width = msg.Info.Width;

                Buffer.BlockCopy(k, 0, data.Map, 0, data.Map.Length);
            });
        }
        public void RegistetyPublisher(string topicName)
        {
            publisher = Ros2Def.node.CreatePublisher<Rosidl.Messages.Nav.OccupancyGrid>(topicName);
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
        public void Publish((sbyte[,] Map, float Resolution, uint Height, uint Width) data)
        {
            this.data = data;
            Publish();
        }
    }
}