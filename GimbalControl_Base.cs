using System.Collections;
using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;
namespace RMUL
{
    public abstract class GimbalControl : MonoBehaviour
    {
        public float angle1, angle2;
        public string Head = "/watcher/decision_maker/rmul/";
        protected ROSConnection ros;
        internal abstract void Work(DecisionMaker.Status status);

        protected enum Mode : byte
        {
            Deg = 0,
            Rad = 1
        }
        /// <summary>
        /// 右手系
        /// </summary>
        /// <param name="vec"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        protected static float Vector3ToAngle(Vector3 vec, Mode mode = Mode.Deg)
        {
            vec.y = 0;
            return Vector3.Angle(Vector3.forward, vec) *
            Mathf.Sign(Vector3.Dot(Vector3.left, vec)) *
            (mode.Equals(Mode.Deg) ? 1 : Mathf.Deg2Rad);
        }
    }
}
