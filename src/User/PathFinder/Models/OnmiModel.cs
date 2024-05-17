using System.Numerics;
using AllianceDM.IO;
using Rosidl.Messages.Geometry;

namespace AllianceDM.CarModels
{
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
        object lock_ = new object();
        public override void Awake()
        {
            circleResolution = float.Parse(Args[0]);
            lieanerResolution = float.Parse(args[1]);
            timeResolution = float.Parse(Args[2]);
            speedlimit = float.Parse(Args[3]);
            accimit = float.Parse(Args[4]);

            IOManager.RegistryMassage(Args[5], (Pose2D msg) => { lock (lock_) { currentSpeed.X = (float)msg.X; currentSpeed.Y = (float)msg.Y; } });
        }
        Vector2[] SpeedLimit()
        {
            lock (lock_)
            {
                List<Vector2> values = [];
                for (float i = 0; i < circleResolution; i++)
                {
                    var head = MathF.SinCos(i / circleResolution * 2 * MathF.PI);
                    for (float j = 0; j < lieanerResolution; j++)
                    {
                        var t = accimit * new Vector2(head.Sin, head.Cos) * j / lieanerResolution * timeResolution;
                        if ((t + currentSpeed).Length() < speedlimit)
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
    }
}