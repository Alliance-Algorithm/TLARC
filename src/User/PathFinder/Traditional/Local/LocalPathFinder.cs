using System.Numerics;

namespace AllianceDM.Nav
{
    public class LocalPathFinder : Component
    {
        protected Vector2 Dir;

        public Vector2 Output => Dir;
    }
}