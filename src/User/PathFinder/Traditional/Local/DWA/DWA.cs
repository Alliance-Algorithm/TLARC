using System.Numerics;
using AllianceDM.CarModels;
using AllianceDM.StdComponent;
using Rosidl.Messages.Geometry;

namespace AllianceDM.Nav
{
    // args[0] = heading coefficient
    // args[1] = obstacle coefficient
    // args[2] = velocity coefficient
    public class DWA : LocalPathFinder
    {
        public float headingCoef;
        public float obstacleCoef;
        public float velocityCoef;

        GlobalPathFinder nav;
        Transform2D sentry;
        CarModel model;
        MapMsg obstacle;

        float currentSpeed = 0;


        float maxValue;

        public override void Start()
        {
        }

        public override void Update()
        {
            var fastpos = new Vector2(-sentry.position.X, sentry.position.Y);
            Dir = nav.Output - fastpos;
            maxValue = 0;
            model.Output.Current.Length();
            Dir = Vector2.Zero;
            Dir = new Vector2(0, 0);
            if (nav.Output.X == 0 && nav.Output.Y == 0)
            {
                return;
            }
            if (model.Output.Sample == null)
                return;
            if (obstacle.Resolution == 0)
                return;
            if ((nav.Output - fastpos).Length() < 1)
            {
                Dir = Rotate(nav.Output - fastpos, -sentry.angle);
            }
            else
            {
                currentSpeed = model.Output.Current.Length();
                foreach (var v in model.Output.Sample)
                {
                    var t = Evaluate(v);
                    if (t > maxValue)
                    {
                        Dir = v;
                        // Dir = v / model.Output.timeResolution;
                        maxValue = t;
                    }
                }
            }
        }

        float Evaluate(Vector2 predict)
        {
            var fastpos = new Vector2(-sentry.position.X, sentry.position.Y);
            float angle = Vector2.Dot(predict, model.Output.Current);
            if (currentSpeed == 0)
                angle = 0;
            else
            {
                angle = angle / (predict).Length() / currentSpeed;
            }

            // float angle2 = Vector2.Dot(Rotate(predict, sentry.Output.angle), nav.Output - fastpos);
            // angle2 = angle2 / predict.Length() / (nav.Output - fastpos).Length();

            Vector2 pos = (predict + model.Output.Current) * 0.5f * model.Output.timeResolution;
            var vv = new Vector2(-pos.Y, -pos.X);
            pos = Rotate(pos, sentry.angle);
            float dis = 100 - (nav.Output - pos - fastpos).Length();
            pos = vv;
            pos = pos / obstacle.Resolution + obstacle.Map.GetLength(1) / 2 * Vector2.One;
            return dis + (1 + angle) * headingCoef + obstacle.Map[(int)pos.X, (int)pos.Y] * obstacleCoef + (predict).Length() * velocityCoef;
        }

        Vector2 Rotate(in Vector2 vec, in double angle)
        {
            var temp = Math.SinCos(angle);
            return new() { X = (float)(vec.X * temp.Cos - vec.Y * temp.Sin), Y = (float)(vec.X * temp.Sin + vec.Y * temp.Cos) };
        }
    }
}