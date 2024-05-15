using System.Numerics;

namespace AllianceDM.CarModels
{
    public class CarModel(uint uuid, uint[] revid, string[] args) : Component(uuid, revid, args)
    {
        public (Vector2 Current, Vector2[] Sample, float timeResolution) Output;
    }
}