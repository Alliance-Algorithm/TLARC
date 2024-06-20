namespace AllianceDM.Nav
{
    public class MapMsg : Component
    {
        protected sbyte[,] _map = new sbyte[0, 0];
        public sbyte[,] Map => _map;
        protected float _resolution;
        public float Resolution => _resolution;
    }
}