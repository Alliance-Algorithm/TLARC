using System.Numerics;

namespace AllianceDM.Nav
{
    public class GlobalPathFinder : Component
    {
        protected Vector2 DestPos;

        public Vector2 Output => DestPos;
    }
}