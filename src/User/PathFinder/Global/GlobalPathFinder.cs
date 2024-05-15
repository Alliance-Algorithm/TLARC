using System.Numerics;

namespace AllianceDM.Nav
{
    public class GlobalPathFinder(uint uuid, uint[] revid, string[] args) : Component(uuid, revid, args)
    {
        protected Vector2 DestPos;

        public Vector2 Output => DestPos;
    }
}