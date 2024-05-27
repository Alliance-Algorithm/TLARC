using AllianceDM.IO;
using Rosidl.Messages.Nav;

// args[0] = map topic
namespace AllianceDM.Nav
{
    public class RasterizedMap(uint uuid, uint[] recvid, string[] args) : MapMsg(uuid, recvid, args)
    {
        public override void Awake()
        {
            Console.WriteLine(string.Format("AllianceDM.Nav RasterizedMap: uuid:{0:D4}", ID));
            IOManager.RegistrySubscription(Args[0], (OccupancyGrid msg) =>
            {
                _map = new sbyte[msg.Info.Height, msg.Info.Width];
                Buffer.BlockCopy(msg.Data, 0, _map, 0, msg.Data.Length);
                _resolution = msg.Info.Resolution;
            });
        }
    }
}