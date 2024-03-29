/// <summary>
/// 消息获取与行为发布
/// </summary>

using System;
using System.Collections.Generic;
using RosMessageTypes.Geometry;
using RosMessageTypes.Nav;
using RosMessageTypes.Std;
using RosMessageTypes.Tf2;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;

namespace RMUL
{
    public class Controller : MonoBehaviour
    {
        public bool tempAutoTime = false;
        public static int WorkIndex = 0;
        delegate DecisionMaker.Status DecisionMakerWork();
        public PathFinder finder;
        public List<DecisionMaker> Makers = new();
        List<DecisionMakerWork> works = new();
        /// <summary>
        /// 增益点是否可用
        /// </summary>
        public static bool GainPoint = true;
        public static int EnemyID = -1;
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
        public static float[] AttackDirs = new float[4];
        // Start is called before the first frame update

        const int armor_id_offset = 0;
        Vector3 chassisForward = new();
        Vector3 gimbalForward = new();

        ROSConnection ros;

        Vector3 beginPos;

        public string Head = "/watcher/decision_maker/rmul/";
        void Start()
        {
            beginPos = transform.position;
            Application.targetFrameRate = 30;
            foreach (var i in Makers)
            {
                works.Add(i.Work);
            }

            #region  QZH
            ros = ROSConnection.GetOrCreateInstance();

            ros.Subscribe(Head + "gain_point_enable", (BoolMsg msg) => GainPoint = msg.data);//增益点是否可用
            ros.Subscribe("gimbal/auto_aim", (Vector3Msg msg) =>
            {
                Vector3 v = new((float)msg.x, (float)msg.y, (float)msg.z);
                if (v == Vector3.zero)
                    EnemyFind = false;
                else
                    EnemyPos = v + transform.position;
            });//是否找到敌人
            ros.Subscribe(Head + "chassis_forward", (Vector3Msg msg) => chassisForward = new((float)msg.x, (float)msg.y, (float)msg.z));

            ros.Subscribe(Head + "lasted_time", (Float32Msg msg) => BeginTime = msg.data);//开局时间
            ros.Subscribe(Head + "hp", (Float32Msg msg) =>
            {
                if (Hp < msg.data)
                    TotalResume += msg.data - Hp;
                Hp = msg.data;
            });//血量
            ros.Subscribe(Head + "bullet_count", (Float32Msg msg) => BulletCount = 550 - msg.data);//剩余弹量
            // ros.Subscribe(Head + "total_resume", (Float32Msg msg) => TotalResume = msg.data);//总回血量
            ros.Subscribe(Head + "enemy_pos", (Vector3Msg msg) =>//最有价值敌人位置
            { EnemyPos = new((float)msg.x, (float)msg.y, (float)msg.z); });
            ros.Subscribe("referee/" + "attack_id", (ByteMsg msg) =>//受攻击方向
            {
                AttackDirCalcu(msg.data - armor_id_offset);
            });

            #endregion
            #region Creeper
            ros.Subscribe("/Odometry", (OdometryMsg msg) =>//自身位置
            {
                Vector3 vec = new(-(float)msg.pose.pose.position.y,
                    (float)msg.pose.pose.position.z,
                    -(float)msg.pose.pose.position.x);
                transform.position = beginPos + vec;

                Quaternion q = new((float)msg.pose.pose.orientation.x,
                (float)msg.pose.pose.orientation.y,
                (float)msg.pose.pose.orientation.z,
                (float)msg.pose.pose.orientation.w);
                chassisForward = q * gimbalForward;
            });
            ros.Subscribe("/gimbal/yaw/angle", (Float64Msg msg) => gimbalForward = Quaternion.AngleAxis(-(float)msg.data, Vector3.up) * Vector3.forward);
            #endregion
        }

        void AttackDirCalcu(int i)
        {

            float num = (float)Math.Sqrt(Vector3.forward.sqrMagnitude * chassisForward.sqrMagnitude);

            float num2 = Mathf.Clamp(Vector3.Dot(chassisForward, Vector3.forward) / num, -1f, 1f);
            float angle = (float)Math.Acos(num2) * 57.29578f * Mathf.Sign(Vector3.Dot(Vector3.forward, chassisForward));
            angle -= 90 * i;

            if (angle < 45 && angle > -45)
                AttackDirs[4] = 1;
            else if (angle <= 135 && angle >= 45)
                AttackDirs[1] = 1;
            else if (angle > 135 || angle < -135)
                AttackDirs[2] = 1;
            else if (angle >= -135 || angle <= -45)
                AttackDirs[3] = 1;
        }
        // internal void Clear()
        // {
        //     AttackDirs.Clear();
        // }

        void Update()
        {
            DecisionMaker.Status State = works[WorkIndex]();
            if (tempAutoTime)
                BeginTime += Time.deltaTime;
            finder.Work(State);

            for (int i = 0; i < 4; i++)
            {
                if (AttackDirs[i] > 0)
                    AttackDirs[i] -= Time.deltaTime;
            }
        }
    }
}