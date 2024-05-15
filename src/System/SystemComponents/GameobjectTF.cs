using System.Diagnostics;
using System.Numerics;
using AllianceDM.IO;
using Rosidl.Messages.Geometry;
using Rosidl.Messages.Nav;

namespace AllianceDM.StdComponent
{
    class Transform2D : Component
    {
        GameObject? gameObject = null;
        Vector2 position;
        double angle;
        Action? action;
        public Transform2D(uint id, uint[] rcvid, string[] args) : base(id, rcvid, args)
        {
            Console.WriteLine(string.Format("Alliance5DM.StdComponent Transform: uuid:{0:D4}", id));
        }
        public override void Awake()
        {
            gameObject = DecisionMaker.FindObject(Args[0]);
            if (Args.Length == 1)
                throw new Exception("Transform Should Declear Read/Write Mode in arg2");
            Args[1] = Args[1].ToUpper();
            if (Args.Length >= 2)
            {
                if (Args[1] == "R")
                {
                    action += () => { position = gameObject.Position; angle = gameObject.Angle; };
                    return;
                }
                else if (Args[1] == "W")
                    action += () => { gameObject.Position = position; gameObject.Angle = angle; };
                else throw new Exception("arg2 should be R,W");
            }
            if (Args[1] == "W" && Args.Length == 3)
            {
                IOManager.RegistryMassage(Args[2], (Odometry msg) => { position = new Vector2(-(float)msg.Pose.Pose.Position.Z, (float)msg.Pose.Pose.Position.X); });
            }
            else throw new Exception("W must declear the topic name\t uuid :" + ID.ToString());
        }

        public override void Update()
        {
            if (action != null)
                action();
        }
        public void Set(Vector2 msg)
        {
            gameObject.Position = msg;
        }
        public (Vector2 pos, double angle) Output => (position, angle);
    }
}