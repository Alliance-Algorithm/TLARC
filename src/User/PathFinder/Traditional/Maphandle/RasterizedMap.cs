using AllianceDM.IO;
using Rosidl.Messages.Nav;

// args[0] = map topic
namespace AllianceDM.Nav
{
    public class RasterizedMap : MapMsg
    {
        public string mapTopicName;
        public override void Start()
        {
            Console.WriteLine(string.Format("AllianceDM.Nav RasterizedMap: uuid:{0:D4}", ID));
            IOManager.RegistrySubscription(mapTopicName, (OccupancyGrid msg) =>
            {
                _map = new sbyte[msg.Info.Height, msg.Info.Width];
                Buffer.BlockCopy(msg.Data, 0, _map, 0, msg.Data.Length);
                _resolution = msg.Info.Resolution;
            });
        }
    }
}