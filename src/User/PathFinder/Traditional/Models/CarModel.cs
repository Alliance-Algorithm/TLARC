using System.Numerics;

namespace AllianceDM.CarModels
{
    public class CarModel : Component
    {
        public (Vector2 Current, Vector2[] Sample, float timeResolution) Output { get; set; }
    }
}