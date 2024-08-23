using Tlarc.IO;
using Tlarc.Utils;
using Rosidl.Messages.Nav;
using Rosidl.Messages.Std;

// args[0] = map topic
// args[1] = BoxSize
// args[2] = ESDF_MaxDistance
namespace Tlarc.Nav
{
    public class ESDFBuilder : MapMsg
    {
        public float boxSize;
        public float ESDF_MaxDistance;
        public string obstacleMapTopicName;

        const float SQRT2 = 1.414213562f;
        WatchDog watchDog;
        IO.ROS2Msgs.Nav.OccupancyGrid pub_Map;
        public override void Start()
        {
            Console.WriteLine(string.Format("AllianceDM.Nav ESDFBuilder: uuid:{0:D4}", ID));
            watchDog = new WatchDog(1.5f, () => { _map = new sbyte[0, 0]; });
            pub_Map = new(IOManager);
            pub_Map.Subscript(obstacleMapTopicName, ((sbyte[,] map, float Resolution, uint Height, uint Width) msg) =>
            {
                if (Map.GetLength(0) != msg.Height)
                    _map = new sbyte[msg.Height, msg.Width];
                Buffer.BlockCopy(msg.map, 0, _map, 0, msg.map.Length);
                _resolution = msg.Resolution;
                MakeESDF(ref _map);

            });
        }

        public override void Update()
        {
            // watchDog.Update();
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