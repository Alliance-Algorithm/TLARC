using System.Collections.Generic;
using System.Linq;
using BEPUphysics;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.CollisionRuleManagement;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using g4;
using RapidlyArmPlanner.ColliderDetector;
using Quaternion = BEPUutilities.Quaternion;
using Space = BEPUphysics.Space;
using Vector3 = BEPUutilities.Vector3;

class NoColliderDetector : IColliderDetector
{
  public bool Detect(List<(Vector3d position, Quaterniond rotation)> poses)
  {
    return false;
  }

  public void Update(
    List<(Vector3d Position, Quaterniond Rotation)> poleTransforms,
    (Vector3d Position, Quaterniond Rotation) redemptionTransform
  )
  {
    return;
  }
}
