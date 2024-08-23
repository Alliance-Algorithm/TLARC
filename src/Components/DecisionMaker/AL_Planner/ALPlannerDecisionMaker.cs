using System.Numerics;

namespace Tlarc.ALPlanner;

class ALPlannerDecisionMaker : Component
{
    public Vector2 TargetPosition = new();
    string firePermitTopicName = "/sentry/fire_permit";
    string lockPermitTopicName = "/sentry/lock_permit";
    string gimbalAngleTopicName = "/sentry/control/angle";
    string targetTopicName = "/sentry/target";

    ADS_FSM FSM;

    IO.ROS2Msgs.Std.Bool firePermitPublisher;
    IO.ROS2Msgs.Std.Int8 lockPermitPublisher;
    IO.ROS2Msgs.Geometry.Pose2D gimbalAnglePublisher;
    IO.ROS2Msgs.Geometry.Pose2D targetPublisher;
    public override void Start()
    {
        firePermitPublisher = new IO.ROS2Msgs.Std.Bool(IOManager);
        firePermitPublisher.RegistryPublisher(firePermitTopicName);
        lockPermitPublisher = new IO.ROS2Msgs.Std.Int8(IOManager);
        lockPermitPublisher.RegistryPublisher(lockPermitTopicName);
        gimbalAnglePublisher = new IO.ROS2Msgs.Geometry.Pose2D(IOManager);
        gimbalAnglePublisher.RegistryPublisher(gimbalAngleTopicName);
        targetPublisher = new IO.ROS2Msgs.Geometry.Pose2D(IOManager);
        targetPublisher.RegistryPublisher(targetTopicName);
    }
    public override void Update()
    {
        firePermitPublisher.Publish(FSM.FirePermit);
        sbyte tempLockPermit = 0;
        for (int i = 0; i < FSM.LockPermit.Length; i++)
            tempLockPermit |= (sbyte)(FSM.LockPermit[i] ? 1 << i : 0);
        lockPermitPublisher.Publish(tempLockPermit);
        gimbalAnglePublisher.Publish((FSM.GimbalAngle, 0));
        targetPublisher.Publish((FSM.TargetPosition, 0));
        TargetPosition = FSM.TargetPosition;
    }
}