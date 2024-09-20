
using DecisionMaker.StateMachine;

namespace DecisionMaker;

class DecisionMaker : Component
{
    public Vector2d TargetPosition;
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
        TargetPosition = FSM.TargetPosition;
        firePermitPublisher.Publish(FSM.FirePermit);
        sbyte tempLockPermit = 0;
        for (int i = 0; i < FSM.LockPermit.Length; i++)
            tempLockPermit |= (sbyte)(FSM.LockPermit[i] ? 1 << i : 0);
        lockPermitPublisher.Publish(tempLockPermit);
        gimbalAnglePublisher.Publish((new((float)FSM.GimbalAngle.x, (float)FSM.GimbalAngle.y), 0));
        targetPublisher.Publish((new((float)FSM.TargetPosition.x, (float)FSM.TargetPosition.y), 0));
    }
}