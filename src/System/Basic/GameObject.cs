using System.Numerics;

namespace AllianceDM
{
    [Serializable]
    public sealed class GameObject
    {
        private float x = 0;
        private float y = 0;
        private double theta = 0;//(1,0)as 0 and contains -180,180
        private string name = "";

        public Vector2 Position { get => new Vector2(x, y); set { x = value.X; y = value.Y; } }
        public double Angle
        {
            get => theta; set
            {
                theta = Math.Abs(value) > Math.PI ? value % Math.Tau : value;
                theta = theta > Math.PI ? theta - Math.Tau : theta;
                theta = theta < -Math.PI ? theta + Math.Tau : theta;
            }
        }
        public string Name => name;

        internal void Log()
        {
            Console.WriteLine("GameObject\t" + name + string.Format(" : x:{0:F2},y:{1:F2},angle:{2:F2}", x, y, theta));
        }
    }
}