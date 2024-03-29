using RosMessageTypes.Std;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;

namespace RMUL
{
    public class GimbalControl_Universal : GimbalControl
    {
        public Transform Dock_Left_Down;
        void Start()
        {
            ros = ROSConnection.GetOrCreateInstance();
            ros.RegisterPublisher<Float32MultiArrayMsg>(Head + "gimbal");
        }
        internal override void Work(DecisionMaker.Status status)
        {
            switch (status)
            {
                case DecisionMaker.Status.Start:
                    angle1 = Vector3ToAngle(Vector3.left);
                    angle2 = Vector3ToAngle(Dock_Left_Down.position - transform.position);
                    break;
                case DecisionMaker.Status.Follow:
                case DecisionMaker.Status.Scan:
                    angle1 = Vector3ToAngle(Controller.EnemyPos - transform.position) + Mathf.PI / 18 / 2;
                    angle2 = angle1 - Mathf.PI / 18;
                    break;

                case DecisionMaker.Status.Escape:
                    int c = 0;
                    foreach (var i in Controller.AttackDirs)
                        c += (i > 0) ? 1 : 0;
                    if (c == 1)
                    {
                        foreach (var i in Controller.AttackDirs)
                        {
                            if (i <= 0)
                                continue;
                            angle1 = Mathf.PI / 2 * i - Mathf.PI / 4;
                            angle2 = Mathf.PI / 2 * i + Mathf.PI / 4;
                        }
                    }
                    else goto default;
                    break;
                case DecisionMaker.Status.Supply:
                    angle1 = -Mathf.PI / 180;
                    angle2 = -angle1;
                    break;
                case DecisionMaker.Status.Standby:
                case DecisionMaker.Status.Cruise:
                case DecisionMaker.Status.Hide:
                default:
                    angle1 = angle2 = 0;
                    break;
            }

            ros.Publish(Head + "gimbal", new Float32MultiArrayMsg(new MultiArrayLayoutMsg(), new float[] { angle1, angle2 }));
        }
    }
}