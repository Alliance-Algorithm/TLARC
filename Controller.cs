/// <summary>
/// 消息获取与行为发布
/// </summary>

using System;
using System.Collections.Generic;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using RosMessageTypes.Tf2;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;

namespace RMUL
{
    public class Controller : MonoBehaviour
    {
        public static int WorkIndex = 0;
        delegate DecisionMaker.Status DecisionMakerWork();
        public PathFinder finder;
        public List<DecisionMaker> Makers = new();
        List<DecisionMakerWork> works = new();
        /// <summary>
        /// 增益点是否可用
        /// </summary>
        public static bool GainPoint = true;
        public static bool EnemyFind = false;
        public static bool EnemySentryDead = false;
        public static bool EnemyIsSentry = false;
        /// <summary>
        /// 开始时间
        /// </summary>
        public static float BeginTime = 0;
        public static float Hp = 600;
        public static float BulletCount = 600;
        /// <summary>
        /// Last Enemy Position We Find
        /// </summary>
        public static float TotalResume = 0;
        public static Vector3 EnemyPos;
        /// <summary>
        /// 被攻击方向
        /// </summary>
        public static HashSet<int> AttckDirs = new HashSet<int>();
        // Start is called before the first frame update


        ROSConnection ros;

        public string Head = "/watcher/decision_maker/rmul/";
        void Start()
        {
            Application.targetFrameRate = 30;
            foreach (var i in Makers)
            {
                works.Add(i.Work);
            }

            #region  QZH
            ros = ROSConnection.GetOrCreateInstance();
            ros.Subscribe(Head + "gain_point_enable", (BoolMsg msg) => GainPoint = msg.data);//增益点是否可用
            ros.Subscribe(Head + "enemy_find", (BoolMsg msg) => EnemyFind = msg.data);//是否找到敌人
            ros.Subscribe(Head + "lasted_time", (Float32Msg msg) => BeginTime = msg.data);//开局时间
            ros.Subscribe(Head + "hp", (Float32Msg msg) => Hp = msg.data);//血量
            ros.Subscribe(Head + "bullet_count", (Float32Msg msg) => BulletCount = msg.data);//剩余弹量
            ros.Subscribe(Head + "total_resume", (Float32Msg msg) => TotalResume = msg.data);//总回血量
            ros.Subscribe(Head + "enemy_pos", (Vector3Msg msg) =>//最有价值敌人位置
            { EnemyPos = new((float)msg.x, (float)msg.y, (float)msg.z); });
            ros.Subscribe(Head + "attack_dir", (Vector3Msg msg) =>//受攻击方向
            {
                Vector3 dir = new((float)msg.x, (float)msg.y, (float)msg.z);
                float num = (float)Math.Sqrt(Vector3.forward.sqrMagnitude * dir.sqrMagnitude);

                float num2 = Mathf.Clamp(Vector3.Dot(dir, Vector3.forward) / num, -1f, 1f);
                float angle = (float)Math.Acos(num2) * 57.29578f * Mathf.Sign(Vector3.Dot(Vector3.right, dir));

                if (angle < 45 && angle > -45)
                    AttckDirs.Add(4);
                else if (angle <= 135 && angle >= 45)
                    AttckDirs.Add(1);
                else if (angle > 135 || angle < -135)
                    AttckDirs.Add(2);
                else if (angle >= -135 || angle <= -45)
                    AttckDirs.Add(3);

            });
            #endregion
            #region Creeper
            ros.Subscribe("/tf", (TFMessageMsg msg) =>//自身位置
            {
                if (msg.transforms[0].header.frame_id != "camera_init")
                    return;
                Vector3 vec = new((float)msg.transforms[0].transform.translation.y,
                (float)msg.transforms[0].transform.translation.z,
                (float)msg.transforms[0].transform.translation.x);
                transform.position = vec;
            });
            #endregion
        }


        internal void Clear()
        {
            AttckDirs.Clear();
        }

        void Update()
        {
            DecisionMaker.Status State = works[WorkIndex]();
            finder.Work(State);
        }
    }
}