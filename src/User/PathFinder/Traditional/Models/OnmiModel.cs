using System.Numerics;
using AllianceDM.IO;
using AllianceDM.StdComponent;
using Rosidl.Messages.Geometry;

namespace AllianceDM.CarModels
{

    public class OmniCar : CarModel
    {
        public float circleResolution;
        public float lieanerResolution;
        public float speedlimit;
        public float accimit;
        public float timeResolution;
        public string currentSpeedTopicName;

        Vector2 currentSpeed;
        Transform2D sentry;
        object lock_ = new object();
        IO.ROS2Msgs.Geometry.Pose2D sub_Speed;
        public override void Start()
        {
            sub_Speed = new();
            sub_Speed.Subscript(currentSpeedTopicName, ((Vector2 pos, float Theta) data) =>
            {
                lock (lock_)
                {
                    currentSpeed.X = (float)data.pos.X; currentSpeed.Y = (float)data.pos.Y;
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
                        t = Rotate(t, -sentry.angle) + currentSpeed;
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