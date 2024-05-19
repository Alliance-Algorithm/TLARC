using System.Numerics;
using AllianceDM.CarModels;
using AllianceDM.StdComponent;
using Rosidl.Messages.Geometry;

namespace AllianceDM.Nav
{
    // args[0] = heading coefficient
    // args[1] = obstacle coefficient
    // args[2] = velocity coefficient
    public class DWA(uint uuid, uint[] revid, string[] args) : LocalPathFinder(uuid, revid, args)
    {
        GlobalPathFinder nav;
        Transform2D sentry;
        CarModel model;
        MapMsg obstacle;

        float headingCoef;
        float obstacleCoef;
        float velocityCoef;

        float currentSpeed = 0;


        float maxValue;

        public override void Awake()
        {
            nav = DecisionMaker.FindComponent<GlobalPathFinder>(RecieveID[0]);
            sentry = DecisionMaker.FindComponent<Transform2D>(RecieveID[1]);
            model = DecisionMaker.FindComponent<CarModel>(RecieveID[2]);
            obstacle = DecisionMaker.FindComponent<MapMsg>(RecieveID[3]);

            headingCoef = float.Parse(Args[0]);
            obstacleCoef = float.Parse(Args[1]);
            velocityCoef = float.Parse(Args[2]);
        }

        public override void Update()
        {
            Dir = nav.Output - sentry.Output.pos;
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
            if ((nav.Output - sentry.Output.pos).Length() < 1)
            {
                Dir = nav.Output - sentry.Output.pos;
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
            Dir = Rotate(Dir, sentry.Output.angle);
        }

        float Evaluate(Vector2 predict)
        {
            float angle = Vector2.Dot(Rotate(predict, sentry.Output.angle) + model.Output.Current, model.Output.Current);
            if (currentSpeed == 0)
                angle = 0;
            else
            {
                angle = angle / (Rotate(predict, sentry.Output.angle) + model.Output.Current).Length() / currentSpeed;
            }

            float angle2 = Vector2.Dot(Rotate(predict, sentry.Output.angle), nav.Output - sentry.Output.pos);
            angle2 = angle2 / predict.Length() / (nav.Output - sentry.Output.pos).Length();

            Vector2 pos = (predict + model.Output.Current + model.Output.Current) * 0.5f * model.Output.timeResolution;
            var vv = new Vector2(-pos.Y, -pos.X);
            float dis = 100 - (nav.Output - pos - sentry.Output.pos).Length();
            pos = Rotate(vv, sentry.Output.angle);
            pos = pos / obstacle.Resolution + obstacle.Map.GetLength(1) / 2 * Vector2.One;
            return dis + (1 + angle) * headingCoef + obstacle.Map[(int)pos.X, (int)pos.Y] * obstacleCoef + predict.Length() * velocityCoef;
        }

        Vector2 Rotate(in Vector2 vec, in double angle)
        {
            var temp = Math.SinCos(angle);
            return new() { X = (float)(vec.X * temp.Cos - vec.Y * temp.Sin), Y = (float)(vec.X * temp.Sin + vec.Y * temp.Cos) };
        }
    }
}