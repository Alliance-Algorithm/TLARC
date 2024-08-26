using System.Numerics;
using Tlarc.CarModels;
using Tlarc.StdComponent;
using Rosidl.Messages.Geometry;

namespace Tlarc.Nav
{
    // args[0] = heading coefficient
    // args[1] = obstacle coefficient
    // args[2] = velocity coefficient
    public class Simple : LocalPathFinder
    {
        public float headingCoef;
        public float obstacleCoef;
        public float velocityCoef;

        GlobalPathFinder nav;
        Transform2D sentry;
        // CarModel model
        // MapMsg obstacle;

        public override void Start()
        {

        }

        public override void Update()
        {
            var fastpos = new Vector2(-sentry.Position.X, sentry.Position.Y);
            Dir = nav.Output - fastpos;
            // model.Output.Current.Length();
            Dir = Vector2.Zero;
            Dir = new Vector2(0, 0);
            if (nav.Output.X == 0 && nav.Output.Y == 0)
            {
                return;
            }
            Dir = nav.Output - fastpos;
            Dir = Rotate(Dir, sentry.angle) / Dir.Length();
        }

        Vector2 Rotate(in Vector2 vec, in double angle)
        {
            var temp = Math.SinCos(angle);
            return new() { X = (float)(vec.X * temp.Cos - vec.Y * temp.Sin), Y = (float)(vec.X * temp.Sin + vec.Y * temp.Cos) };
        }
    }
}