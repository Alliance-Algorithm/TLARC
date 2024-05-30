using System.Numerics;
using AllianceDM.IO;
using Rcl;
using Rosidl.Messages.Geometry;
using Rosidl.Messages.Std;

namespace AllianceDM.IO.ROS2Msgs.Nav
{
    class OccupancyGrid : TlarcMsgs
    {
<<<<<<< HEAD
        public delegate void RevcAction((sbyte[,] Map, float Resolution, int Height, int Width) msg);
        (sbyte[,] Map, float Resolution, int Height, int Width) data;
        RevcAction callback;
=======
        (sbyte[,] Map, float Resolution, uint Height, uint Width) data;
        RevcAction<(sbyte[,] Map, float Resolution, uint Height, uint Width)> callback;
>>>>>>> refs/remotes/origin/main

        static protected bool WriteLock = false;

        IRclPublisher<Rosidl.Messages.Nav.OccupancyGrid> publisher;
        Rcl.RosMessageBuffer nativeMsg;

        void Subscript()
        {
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
        public void Subscript(string topicName, RevcAction<(sbyte[,] Map, float Resolution, uint Height, uint Width)> callback)
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

                Buffer.BlockCopy(k, 0, data.Map, 0, data.Map.Length);
            });
        }
        public void RegistetyPublisher(string topicName)
        {
            publisher = Ros2Def.node.CreatePublisher<Rosidl.Messages.Nav.OccupancyGrid>(topicName);
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
                    publisher.Publish(nativeMsg);
                    WriteLock = false;
                    await timer.WaitOneAsync(false);
                }
            });
        }
        public void Publish((sbyte[,] Map, float Resolution, int Height, int Width) data)
        {
            this.data = data;
        }
    }
}