namespace AllianceDM.Nav
{
    public class MapMsg(uint uuid, uint[] revid, string[] args) : Component(uuid, revid, args)
    {
        protected sbyte[,] _map = new sbyte[0, 0];
        public sbyte[,] Map => _map;
        protected float _resolution;
        public float Resolution => _resolution;
    }
}