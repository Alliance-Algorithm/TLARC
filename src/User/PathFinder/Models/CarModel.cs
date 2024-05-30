using System.Numerics;

namespace AllianceDM.CarModels
{
    public class CarModel(uint uuid, uint[] revid, string[] b_args) : Component(uuid, revid, b_args)
    {
        public (Vector2 Current, Vector2[] Sample, float timeResolution) Output;
    }
}