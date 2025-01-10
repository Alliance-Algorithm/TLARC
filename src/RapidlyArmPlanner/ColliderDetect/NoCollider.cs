using BEPUphysics;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.NarrowPhaseSystems.Pairs;

using BEPUphysics.CollisionRuleManagement;
using System.Collections.Generic;
using g4;


using Vector3 = BEPUutilities.Vector3;
using Quaternion = BEPUutilities.Quaternion;
using Space = BEPUphysics.Space;
using System.Linq;
using RapidlyArmPlanner.ColliderDetector;
class NoColliderDetector : IColliderDetector
{
    public bool Detect(List<(Vector3d position, Quaterniond rotation)> poses)
    {
        return false;
    }

    public void Update(List<(Vector3d Position, Quaterniond Rotation)> poleTransforms, (Vector3d Position, Quaterniond Rotation) redemptionTransform)
    {
        return;
    }
}