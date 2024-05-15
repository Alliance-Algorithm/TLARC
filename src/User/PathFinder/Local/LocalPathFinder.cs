using System.Numerics;

namespace AllianceDM.Nav
{
    public class LocalPathFinder(uint uuid, uint[] revid, string[] args) : Component(uuid, revid, args)
    {
        protected Vector2 Dir;

        public Vector2 Output => Dir;
    }
}