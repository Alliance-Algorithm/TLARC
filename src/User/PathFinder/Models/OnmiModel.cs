using System.Numerics;
using AllianceDM.IO;
using AllianceDM.StdComponent;
using Rosidl.Messages.Geometry;

namespace AllianceDM.CarModels
{
    // input[0] = sentry TF
    // args[0] = circle resolution
    // args[1] = lieaner resolution
    // args[2] = time resolution
    // args[3] = max speed
    // args[4] = max acc
    // args[5] = speed topic

    public class OmniCar(uint uuid, uint[] revid, string[] args) : CarModel(uuid, revid, args)
    {
        float circleResolution;
        float lieanerResolution;
        float speedlimit;
        float accimit;

        float timeResolution;

        Vector2 currentSpeed;
        Transform2D sentry;
        object lock_ = new object();
        IO.ROS2Msgs.Geometry.Pose2D sub_Speed;
        public override void Awake()
        {
            sentry = DecisionMaker.FindComponent<Transform2D>(RecieveID[0]);
            circleResolution = float.Parse(Args[0]);
            lieanerResolution = float.Parse(args[1]);
            timeResolution = float.Parse(Args[2]);
            speedlimit = float.Parse(Args[3]);
            accimit = float.Parse(Args[4]);
            sub_Speed = new();
            sub_Speed.Subscript(Args[5], ((Vector2 pos, float Theta) data) => { });
            IOManager.RegistrySubscription(Args[5], (Pose2D msg) =>
            {
                lock (lock_)
                {
                    currentSpeed.X = (float)msg.X; currentSpeed.Y = (float)msg.Y;
                }
            });
        }
        Vector2[] SpeedLimit()
        {
            lock (lock_)
            {
                List<Vector2> values = [];
                // values.Add(Vector2.Zero);
                for (float i = 0; i < circleResolution; i++)
                {
                    var head = MathF.SinCos(i / circleResolution * 2 * MathF.PI);
                    for (float j = 0; j < lieanerResolution; j++)
                    {
                        var t = accimit * new Vector2(head.Sin, head.Cos) * j / lieanerResolution;
                        t = Rotate(t, -sentry.Output.angle) + currentSpeed;
                        if (t.Length() < speedlimit)
                            values.Add(t);
                    }
                }
                return [.. values];
            }
        }
        public override void Update()
        {
            Output = (currentSpeed, SpeedLimit(), timeResolution);
        }
        Vector2 Rotate(in Vector2 vec, in double angle)
        {
            var temp = Math.SinCos(angle);
            return new() { X = (float)(vec.X * temp.Cos - vec.Y * temp.Sin), Y = (float)(vec.X * temp.Sin + vec.Y * temp.Cos) };
        }
    }
}