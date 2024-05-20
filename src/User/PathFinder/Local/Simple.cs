using System.Numerics;
using AllianceDM.CarModels;
using AllianceDM.StdComponent;
using Rosidl.Messages.Geometry;

namespace AllianceDM.Nav
{
    // args[0] = heading coefficient
    // args[1] = obstacle coefficient
    // args[2] = velocity coefficient
    public class Simple(uint uuid, uint[] revid, string[] args) : LocalPathFinder(uuid, revid, args)
    {
        GlobalPathFinder nav;
        Transform2D sentry;
        // CarModel model
        // MapMsg obstacle;

        float headingCoef;
        float obstacleCoef;
        float velocityCoef;

        float currentSpeed = 0;


        float maxValue;

        public override void Awake()
        {
            nav = DecisionMaker.FindComponent<GlobalPathFinder>(RecieveID[0]);
            sentry = DecisionMaker.FindComponent<Transform2D>(RecieveID[1]);
            // model = DecisionMaker.FindComponent<CarModel>(RecieveID[2]);
            // obstacle = DecisionMaker.FindComponent<MapMsg>(RecieveID[3]);

            headingCoef = float.Parse(Args[0]);
            obstacleCoef = float.Parse(Args[1]);
            velocityCoef = float.Parse(Args[2]);
        }

        public override void Update()
        {
            Dir = nav.Output - sentry.Output.pos;
            maxValue = 0;
            // model.Output.Current.Length();
            Dir = Vector2.Zero;
            Dir = new Vector2(0, 0);
            if (nav.Output.X == 0 && nav.Output.Y == 0)
            {
                return;
            }
            Dir = nav.Output - sentry.Output.pos;
            Dir = Rotate(Dir, sentry.Output.angle) / Dir.Length();
        }

        Vector2 Rotate(in Vector2 vec, in double angle)
        {
            var temp = Math.SinCos(angle);
            return new() { X = (float)(vec.X * temp.Cos - vec.Y * temp.Sin), Y = (float)(vec.X * temp.Sin + vec.Y * temp.Cos) };
        }
    }
}