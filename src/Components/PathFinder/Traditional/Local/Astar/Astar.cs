using System.Numerics;
using Tlarc.StdComponent;

namespace Tlarc.Nav
{
    public class Astar : LocalPathFinder
    {
        GlobalPathFinder nav;
        Transform2D sentry;

        public override void Start()
        {
        }
        public override void Update()
        {
            Dir = nav.Output - sentry.position;
        }
    }
}