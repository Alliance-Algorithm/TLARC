using RosMessageTypes.Std;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;

namespace RMUL
{
    public class GimbalControl_Advantageous : GimbalControl
    {
        public Transform Dock_Left_Down;
        void Start()
        {
            ros = ROSConnection.GetOrCreateInstance();
            ros.RegisterPublisher<Float32MultiArrayMsg>(Head + "Gimbal");
        }
        internal override void Work(DecisionMaker.Status status)
        {
            switch (status)
            {
                case DecisionMaker.Status.Start:
                    angle1 = Vector3ToAngle(Vector3.left);
                    angle2 = Vector3ToAngle(Dock_Left_Down.position - transform.position);
                    break;
                case DecisionMaker.Status.AttackBase:
                case DecisionMaker.Status.Follow:
                    angle1 = angle2 = -1;
                    break;
                case DecisionMaker.Status.Escape:
                    if (Controller.EnemyFind) angle1 = angle2 = -1;
                    else goto default;
                    break;
                case DecisionMaker.Status.Supply:
                case DecisionMaker.Status.Standby:
                case DecisionMaker.Status.Cruise:
                case DecisionMaker.Status.Hide:
                default:
                    angle1 = angle2 = 0;
                    break;
            }

            ros.Publish(Head + "Gimbal", new Float32MultiArrayMsg(new MultiArrayLayoutMsg(), new float[] { angle1, angle2 }));
        }
    }
}