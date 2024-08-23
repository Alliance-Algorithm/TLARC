using System.Numerics;

namespace Tlarc.Nav
{
    public class GlobalPathFinder : Component
    {
        protected Vector2 DestPos;

        public Vector2 Output => DestPos;
    }
}