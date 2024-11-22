using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.NarrowPhaseSystems.Pairs;

using BEPUphysics.CollisionRuleManagement;

namespace RapidlyArmPlanner.ColliderDetector;


using Vector3 = BEPUutilities.Vector3;
using Quaternion = BEPUutilities.Quaternion;
using Space = BEPUphysics.Space;
using System.Linq;
class Scara2025BepuDetector : IColliderDetector
{
    Space space;
    List<Entity> poles;
    List<Entity> redemptions;
    int[] flags;
    public Scara2025BepuDetector()
    {
        space = new Space();
        flags = new int[5];
        space.ForceUpdater.Gravity = Vector3.Zero;
        poles =
        [
            new Box(Vector3.Zero, 0.05f, 0.05f, 1, 1),
            new Box(Vector3.Zero, 0.3f, 0.05f,0.05f, 1),
            new Box(Vector3.Zero, 0.3f, 0.05f, 0.05f, 1),
            new Box(Vector3.Zero, 0.1f, 0.05f, 0.05f, 1),
            new Box(Vector3.Zero, 0.24f, 0.24f, 0.24f, 1)
        ];
        foreach (var pole in poles)
        {
            pole.CollisionInformation.CollisionRules.Personal = CollisionRule.NoSolver;
            space.Add(pole);
        }

        redemptions = new List<Entity>()
        {
            new Box(new Vector3(1.11f,1.12f,1), 0.21f, 0.02f, 0.21f, 1),
            new Box(new Vector3(1.11f,-1.12f,1), 0.21f, 0.02f, 0.21f, 1),
            new Box(new Vector3(1.11f,1f,1.12f), 0.21f, 0.23f, 0.02f, 1),
            new Box(new Vector3(1.11f,1f,-1.12f), 0.21f, 0.23f, 0.02f, 1),
            new Box(new Vector3(1.23f,1,1), 0.02f, 0.24f, 0.24f, 1)
        };
        int i = 0;
        foreach (var redemption in redemptions)
        {
            redemption.Tag = i++;
            redemption.CollisionInformation.CollisionRules.Personal = CollisionRule.NoSolver;
            redemption.CollisionInformation.Events.InitialCollisionDetected += HandleDetectCollision;
            redemption.CollisionInformation.Events.CollisionEnding += HandleCollision;
            space.Add(redemption);
        }

    }
    internal void Update(List<(Vector3d Position, Quaterniond Rotation)> poleTransforms, (Vector3d Position, Quaterniond Rotation) redemptionTransform)
    {
        Vector3d X = redemptionTransform.Rotation * Vector3d.AxisX;
        Vector3d Y = redemptionTransform.Rotation * Vector3d.AxisY;
        Vector3d Z = redemptionTransform.Rotation * Vector3d.AxisZ;

        Vector3 axisX = new Vector3((float)X.x, (float)X.y, (float)X.z);
        Vector3 axisY = new Vector3((float)Y.x, (float)Y.y, (float)Y.z);
        Vector3 axisZ = new Vector3((float)Z.x, (float)Z.y, (float)Z.z);

        Vector3 position = new Vector3((float)redemptionTransform.Position.x, (float)redemptionTransform.Position.y, (float)redemptionTransform.Position.z);
        Quaternion quaternion = new Quaternion((float)redemptionTransform.Rotation.x, (float)redemptionTransform.Rotation.y, (float)redemptionTransform.Rotation.z, (float)redemptionTransform.Rotation.w);

        redemptions[0].Position = position + axisX * 0.11f + axisY * 0.12f;
        redemptions[0].Orientation = quaternion;
        redemptions[1].Position = position + axisX * 0.11f - axisY * 0.12f;
        redemptions[1].Orientation = quaternion;
        redemptions[2].Position = position + axisX * 0.11f + axisZ * 0.12f;
        redemptions[2].Orientation = quaternion;
        redemptions[3].Position = position + axisX * 0.11f - axisZ * 0.12f;
        redemptions[3].Orientation = quaternion;
        redemptions[4].Position = position + axisX * 0.23f;
        redemptions[4].Orientation = quaternion;

        for (int i = 0; i < 5; i++)
        {
            poles[i].Position = new Vector3((float)poleTransforms[i].Position.x, (float)poleTransforms[i].Position.y, (float)poleTransforms[i].Position.z);
            poles[i].Orientation = new Quaternion((float)poleTransforms[i].Rotation.x, (float)poleTransforms[i].Rotation.y, (float)poleTransforms[i].Rotation.z, (float)poleTransforms[i].Rotation.w);
        }

        space.Update();
    }
    private void HandleDetectCollision(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
    {
        flags[(int)sender.Entity.Tag]++;
    }

    private void HandleCollision(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
    {
        flags[(int)sender.Entity.Tag]--;
    }

    public bool Detect(List<(Vector3d position, Quaterniond rotation)> poses)
    {
        for (int i = 0; i < 5; i++)
        {
            poles[i].Position = new Vector3((float)poses[i].position.x, (float)poses[i].position.y, (float)poses[i].position.z);
            poles[i].Orientation = new Quaternion((float)poses[i].rotation.x, (float)poses[i].rotation.y, (float)poses[i].rotation.z, (float)poses[i].rotation.w);
        }

        space.Update();

        return !flags.All(b => b == 0);
    }
}