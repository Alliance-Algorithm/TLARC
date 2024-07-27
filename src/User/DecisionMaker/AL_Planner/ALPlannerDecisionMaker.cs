using System.Numerics;

namespace AllianceDM.ALPlanner;

class ALPlannerDecisionMaker : Component
{
    public Vector2 TargetPosition => FSM.TargetPosition;
    public string firePermitTopicName = "/sentry/fire_permit";
    public string lockPermitTopicName = "/sentry/lock_permit";
    public string gimbalAngleTopicName = "/sentry/control/angle";
    public string targetTopicName = "/sentry/target";

    ADS_FSM FSM;

    IO.ROS2Msgs.Std.Bool firePermitPublisher;
    IO.ROS2Msgs.Std.Int8 lockPermitPublisher;
    IO.ROS2Msgs.Geometry.Pose2D gimbalAnglePublisher;
    IO.ROS2Msgs.Geometry.Pose2D targetPublisher;
    public override void Start()
    {
        firePermitPublisher = new IO.ROS2Msgs.Std.Bool();
        firePermitPublisher.RegistryPublisher(firePermitTopicName);
        lockPermitPublisher = new IO.ROS2Msgs.Std.Int8();
        lockPermitPublisher.RegistryPublisher(lockPermitTopicName);
        gimbalAnglePublisher = new IO.ROS2Msgs.Geometry.Pose2D();
        gimbalAnglePublisher.RegistryPublisher(gimbalAngleTopicName);
        targetPublisher = new IO.ROS2Msgs.Geometry.Pose2D();
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
    }
}