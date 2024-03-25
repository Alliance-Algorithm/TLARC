/// <summary>
/// 得到行为，找出路径
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RMUL
{
    using System;
    using System.Linq;
    using RosMessageTypes.Geometry;
    using RosMessageTypes.Std;
    using Unity.Robotics.ROSTCPConnector;
    using Unity.VisualScripting;
    using UnityEngine;
    using UnityEngine.AI;
    using UnityEngine.UIElements;

    [RequireComponent(typeof(NavMeshAgent))]
    public class PathFinder_Universal : PathFinder
    {

        public string Head = "/watcher/decision_maker/rmul/";
        ROSConnection ros;
        internal override Vector3 targetPos
        {
            get { return _targetPos; }
            set
            {
                if (value == _targetPos)
                    return;
                _targetPos = value;
                agent.SetDestination(_targetPos);
                if (!agent.CalculatePath(_targetPos, _path))
                {
                    Debug.Log(targetPos);
                    Debug.LogError("agentCalculatePath Failed at PathFinder");
                }
            }
        }
        internal override Vector2 dest
        {
            get
            {
                targetPos = targetPos;
                if (_path == null)
                    return Vector2.zero;
                else if (_path.corners.Length <= 1)
                    return Vector2.zero;
                Vector3 v = _path.corners[1] - transform.position;
                return new Vector2(v.x, v.z).normalized;
            }
        }

        void Start()
        {
            _path = new NavMeshPath();
            agent = GetComponent<NavMeshAgent>();

            ros = ROSConnection.GetOrCreateInstance();
            ros.RegisterPublisher<Vector3Msg>(Head + "dest_dir");

        }

        void Update()
        {
            if (Input.GetMouseButtonUp(0))
            {
                //需要碰撞到物体才可以
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                bool isCollider = Physics.Raycast(ray, out hit);
                if (isCollider)
                {
                    targetPos = hit.point;
                }
            }
        }

        /// <summary>
        /// For RMUL
        /// </summary>

        public Transform StartMoveTo;
        public Transform HidePosition;
        public Transform StandByPosition;
        public Transform SupplyPos;

        ///0 center,1 left,2 down,3 right,4 up 
        const int center = 0, left = 1, down = 2, right = 3, up = 4;
        const int bestDis = 3;
        const int leftX = -10, rightX = 10, upY = 4, downY = -4;
        internal int[,][] AreaConnect = {
            {new[]{1},new[]{-1},new[]{-1},new[]{-1},new[]{-1}},
            {new[]{2},new[]{-1},new[]{4},new[]{-1},new[]{3}},
            {new[]{3},new[]{1},new[]{2,4},new[]{5},new[]{-1}},
            {new[]{4},new[]{1},new[]{-1},new[]{6},new[]{2,3}},
            {new[]{5},new[]{3,1},new[]{6},new[]{-1},new[]{-1}},
            {new[]{6},new[]{4},new[]{-1},new[]{-1},new[]{5}}
        };
        internal Vector3[] Positions = new Vector3[7];
        internal override void Work(DecisionMaker.Status state)
        {
            switch (state)
            {
                case DecisionMaker.Status.Start:
                    targetPos = StartMoveTo.position;
                    break;

                case DecisionMaker.Status.Hide:
                    targetPos = HidePosition.position;
                    break;
                case DecisionMaker.Status.Standby:
                    targetPos = StandByPosition.position;
                    break;

                case DecisionMaker.Status.Supply:
                    targetPos = SupplyPos.position;
                    break;

                case DecisionMaker.Status.Cruise:
                    if ((transform.position - Positions[3]).magnitude < 1)
                        targetPos = Positions[4];
                    else if ((transform.position - Positions[4]).magnitude < 1)
                        targetPos = Positions[3];
                    else if (targetPos != Positions[3] && targetPos != Positions[4])
                        if ((transform.position - Positions[3]).magnitude < (transform.position - Positions[4]).magnitude)
                            targetPos = Positions[3];
                        else
                            targetPos = Positions[4];
                    break;

                case DecisionMaker.Status.Follow:
                    if ((transform.position - Controller.EnemyPos).magnitude < bestDis)
                        return;
                    targetPos = Controller.EnemyPos - (transform.position - Controller.EnemyPos).normalized * bestDis;
                    break;

                case DecisionMaker.Status.Scan:
                    targetPos = Controller.EnemyPos;
                    break;

                case DecisionMaker.Status.Escape:
                    int i = LocationIndex();
                    List<int> dangrous = new();
                    foreach (var k in Controller.AttckDirs)
                        dangrous.AddRange(AreaConnect[i, k]);
                    dangrous = dangrous.Distinct().ToList();
                    dangrous.Sort();
                    i = dangrous[1];

                    targetPos = Positions[Mathf.Clamp(i - 1, 1, 6)];
                    break;


            }


            if ((targetPos - transform.position).magnitude < 0.5f)
                return;
            ros.Publish(Head + "dest_dir", new Vector3Msg(-dest.y, dest.x, 0));
        }

        int LocationIndex()
        {
            float x = transform.position.x, z = transform.position.z;
            if (x < leftX)
            {
                if (z > upY) return 5;
                else return 6;
            }
            else if (x > rightX)
            {
                return 1;
            }
            else
            {
                if (z > upY) return 3;
                else if (z < downY) return 4;
                else return 2;
            }



        }

    }
}
