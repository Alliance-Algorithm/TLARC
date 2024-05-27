using AllianceDM.IO;
using AllianceDM.Utils;
using Rosidl.Messages.Nav;
using Rosidl.Messages.Std;

// args[0] = map topic
// args[1] = BoxSize
// args[2] = ESDF_MaxDistance
namespace AllianceDM.Nav
{
    public class ESDFBuilder(uint uuid, uint[] recvid, string[] args) : MapMsg(uuid, recvid, args)
    {
        float boxSize;
        float ESDF_MaxDistance;
        const float SQRT2 = 1.414213562f;
        object lock_ = new object();
        WatchDog watchDog;
        public override void Awake()
        {
            watchDog = new WatchDog(1.5f, () => { _map = new sbyte[0, 0]; });
            IOManager.RegistrySubscription(Args[0], (OccupancyGrid msg) =>
            {
                watchDog.Feed();
                lock (lock_)
                {
                    if (Map.GetLength(0) != msg.Info.Height)
                        _map = new sbyte[msg.Info.Height, msg.Info.Width];
                    Buffer.BlockCopy(msg.Data, 0, _map, 0, msg.Data.Length);
                    _resolution = msg.Info.Resolution;
                    MakeESDF(ref _map);
                }
            });

            boxSize = float.Parse(Args[1]);
            ESDF_MaxDistance = float.Parse(Args[2]);
            // Task.Run(async () =>
            // {
            //     using var pub = Ros2Def.node.CreatePublisher<OccupancyGrid>("/map_test");
            //     using var nativeMsg = pub.CreateBuffer();
            //     using var timer = Ros2Def.context.CreateTimer(Ros2Def.node.Clock, TimeSpan.FromMilliseconds(value: 1000 / 1));
            //     while (true)
            //     {
            //         var temp_map = new sbyte[Map.Length];
            //         Buffer.BlockCopy(Map, 0, temp_map, 0, temp_map.Length);
            //         nativeMsg.AsRef<OccupancyGrid.Priv>().Data.CopyFrom(temp_map);
            //         nativeMsg.AsRef<OccupancyGrid.Priv>().Info.Height = (uint)Map.GetLength(0);
            //         nativeMsg.AsRef<OccupancyGrid.Priv>().Info.Width = (uint)Map.GetLength(1);
            //         nativeMsg.AsRef<OccupancyGrid.Priv>().Info.Resolution = Resolution;
            //         nativeMsg.AsRef<OccupancyGrid.Priv>().Header = new Header.Priv() { FrameId = new Rosidl.Runtime.Interop.CString(new ReadOnlySpan<sbyte>([(sbyte)'m', (sbyte)'a', (sbyte)'p', (sbyte)'_', (sbyte)'2', (sbyte)'d', (sbyte)'_', (sbyte)'l', (sbyte)'i', (sbyte)'n', (sbyte)'k'])) };
            //         pub.Publish(nativeMsg);
            //         await timer.WaitOneAsync(false);
            //     }
            // });
        }

        public override void Update()
        {
            // watchDog.Update();
            lock (lock_) ;
        }

        public void MakeESDF(ref sbyte[,] map)
        {
            if (map == null || map.Length == 0)
                return;
            Queue<(int x, int y)> queue = new();
            float boxSize_ = boxSize * Resolution;

            for (int x = 0; x < boxSize; x += 1)
            {
                for (int y = 0; y < boxSize; y += 1)
                {
                    if (map[x, y] == 0)
                        map[x, y] = 0;
                    else
                        map[x, y] = 100;

                }
            }
            for (float x = Resolution; x < boxSize_; x += Resolution)
            {
                for (float y = Resolution; y < boxSize_; y += Resolution)
                {
                    (int x, int y) pos = ((int)(x / Resolution), (int)(y / Resolution));
                    if (map[pos.x, pos.y] == 0)
                    {
                        if (map[pos.x + 1, pos.y] != 0)
                        {
                            map[pos.x + 1, pos.y] = Math.Min(map[pos.x + 1, pos.y], (sbyte)(Resolution / ESDF_MaxDistance * 100));
                            queue.Enqueue((pos.x + 1, pos.y));
                        }
                        if (map[pos.x - 1, pos.y] != 0)
                        {
                            map[pos.x - 1, pos.y] = Math.Min(map[pos.x - 1, pos.y], (sbyte)(Resolution / ESDF_MaxDistance * 100));
                            queue.Enqueue((pos.x - 1, pos.y));
                        }
                        if (map[pos.x, pos.y + 1] != 0)
                        {
                            map[pos.x, pos.y + 1] = Math.Min(map[pos.x, pos.y + 1], (sbyte)(Resolution / ESDF_MaxDistance * 100));
                            queue.Enqueue((pos.x, pos.y + 1));
                        }
                        if (map[pos.x, pos.y - 1] != 0)
                        {
                            map[pos.x, pos.y - 1] = Math.Min(map[pos.x, pos.y - 1], (sbyte)(Resolution / ESDF_MaxDistance * 100));
                            queue.Enqueue((pos.x, pos.y - 1));
                        }
                        if (map[pos.x + 1, pos.y + 1] != 0)
                        {
                            map[pos.x + 1, pos.y + 1] = Math.Min(map[pos.x + 1, pos.y + 1], (sbyte)(SQRT2 * Resolution / ESDF_MaxDistance * 100));
                            queue.Enqueue((pos.x + 1, pos.y + 1));
                        }
                        if (map[pos.x - 1, pos.y - 1] != 0)
                        {
                            map[pos.x - 1, pos.y - 1] = Math.Min(map[pos.x - 1, pos.y - 1], (sbyte)(SQRT2 * Resolution / ESDF_MaxDistance * 100));
                            queue.Enqueue((pos.x - 1, pos.y - 1));
                        }
                        if (map[pos.x - 1, pos.y + 1] != 0)
                        {
                            map[pos.x - 1, pos.y + 1] = Math.Min(map[pos.x - 1, pos.y + 1], (sbyte)(SQRT2 * Resolution / ESDF_MaxDistance * 100));
                            queue.Enqueue((pos.x - 1, pos.y + 1));
                        }
                        if (map[pos.x + 1, pos.y - 1] != 0)
                        {
                            map[pos.x + 1, pos.y - 1] = Math.Min(map[pos.x + 1, pos.y - 1], (sbyte)(SQRT2 * Resolution / ESDF_MaxDistance * 100));
                            queue.Enqueue((pos.x + 1, pos.y - 1));
                        }
                    }
                }
            }

            while (queue.Count > 0)
            {

                (int x, int y) pos = queue.Dequeue();
                if (map[pos.x, pos.y] != 100)
                {
                    if (pos.x + 1 < (int)Math.Ceiling(boxSize_ / Resolution) && map[pos.x + 1, pos.y] == 100)
                    {
                        uint temp = (uint)map[pos.x, pos.y] + (uint)(Resolution / ESDF_MaxDistance * 100);
                        map[pos.x + 1, pos.y] = (sbyte)Math.Clamp(temp, 0, 100);
                        queue.Enqueue((pos.x + 1, pos.y));
                    }
                    if (pos.x - 1 >= 0 && map[pos.x - 1, pos.y] == 100)
                    {
                        uint temp = (uint)map[pos.x, pos.y] + (uint)(Resolution / ESDF_MaxDistance * 100);
                        map[pos.x - 1, pos.y] = (sbyte)Math.Clamp(temp, 0, 100);
                        queue.Enqueue((pos.x - 1, pos.y));
                    }
                    if (pos.y + 1 < (int)Math.Ceiling(boxSize_ / Resolution) && map[pos.x, pos.y + 1] == 100)
                    {
                        uint temp = (uint)map[pos.x, pos.y] + (uint)(Resolution / ESDF_MaxDistance * 100);
                        map[pos.x, pos.y + 1] = (sbyte)Math.Clamp(temp, 0, 100);
                        queue.Enqueue((pos.x, pos.y + 1));
                    }
                    if (pos.y - 1 >= 0 && map[pos.x, pos.y - 1] == 100)
                    {
                        uint temp = (uint)map[pos.x, pos.y] + (uint)(Resolution / ESDF_MaxDistance * 100);
                        map[pos.x, pos.y - 1] = (sbyte)Math.Clamp(temp, 0, 100);
                        queue.Enqueue((pos.x, pos.y - 1));
                    }
                    if (pos.x + 1 < (int)Math.Ceiling(boxSize_ / Resolution) && pos.y + 1 < (int)Math.Ceiling(boxSize_ / Resolution) && map[pos.x + 1, pos.y + 1] == 100)
                    {
                        uint temp = (uint)map[pos.x, pos.y] + (uint)Math.Ceiling(SQRT2 * Resolution / ESDF_MaxDistance * 100);
                        map[pos.x + 1, pos.y + 1] = (sbyte)Math.Clamp(temp, 0, 100);
                        queue.Enqueue((pos.x + 1, pos.y + 1));
                    }
                    if (pos.x - 1 >= 0 && pos.y - 1 >= 0 && map[pos.x - 1, pos.y - 1] == 100)
                    {
                        uint temp = (uint)map[pos.x, pos.y] + (uint)Math.Ceiling(SQRT2 * Resolution / ESDF_MaxDistance * 100);
                        map[pos.x - 1, pos.y - 1] = (sbyte)Math.Clamp(temp, 0, 100);
                        queue.Enqueue((pos.x - 1, pos.y - 1));
                    }
                    if (pos.y + 1 < (int)Math.Ceiling(boxSize_ / Resolution) && pos.x - 1 >= 0 && map[pos.x - 1, pos.y + 1] == 100)
                    {
                        uint temp = (uint)map[pos.x, pos.y] + (uint)Math.Ceiling(SQRT2 * Resolution / ESDF_MaxDistance * 100);
                        map[pos.x - 1, pos.y + 1] = (sbyte)Math.Clamp(temp, 0, 100);
                        queue.Enqueue((pos.x - 1, pos.y + 1));
                    }
                    if (pos.x + 1 < (int)Math.Ceiling(boxSize_ / Resolution) && pos.y - 1 >= 0 && map[pos.x + 1, pos.y - 1] == 100)
                    {
                        uint temp = (uint)(map[pos.x, pos.y] + (uint)Math.Ceiling(SQRT2 * Resolution / ESDF_MaxDistance * 100));
                        map[pos.x + 1, pos.y - 1] = (sbyte)Math.Clamp(temp, 0, 100);
                        queue.Enqueue((pos.x + 1, pos.y - 1));
                    }
                }
            }

        }
    }
}