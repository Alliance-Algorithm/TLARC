using System.Numerics;
using Tlarc.IO;
using Tlarc.StdComponent;
using Rosidl.Messages.Geometry;

namespace Tlarc.CarModels
{

    public class OmniCar : CarModel
    {
        float circleResolution;
        float linerResolution;
        float speedLimit;
        float accLimit;
        float timeResolution;
        string currentSpeedTopicName;

        Vector2 currentSpeed;
        Transform2D sentry;
        object lock_ = new object();
        IO.ROS2Msgs.Geometry.Pose2D sub_Speed;
        public override void Start()
        {
            sub_Speed = new(IOManager);
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
                    for (float j = 0; j < linerResolution; j++)
                    {
                        var t = accLimit * new Vector2(head.Sin, head.Cos) * j / linerResolution;
                        t = Rotate(t, -sentry.angle) + currentSpeed;
                        if (t.Length() < speedLimit)
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