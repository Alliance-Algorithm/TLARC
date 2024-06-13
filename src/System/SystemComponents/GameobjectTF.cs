using System.Diagnostics;
using System.Numerics;
using AllianceDM.IO;
using Rosidl.Messages.Geometry;
using Rosidl.Messages.Nav;

namespace AllianceDM.StdComponent
{
    public class Transform2D : Component
    {

        public string name;
        public Vector2 position;
        public double angle;

        public override void Start()
        {

        }

        public override void Update()
        {
        }
        public void Set(Vector2 msg)
        {
            position = msg;
        }
    }
}