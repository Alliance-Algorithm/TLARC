/// <summary>
/// 打上交等强队的决策树
/// </summary>

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace RMUL
{
    class DecisionMaker_Disadvantageous : DecisionMaker
    {
        static Status State = Status.Start;

        private const float BeginTimeMax = 30;
        private const float HpMin = 100;
        private const float BulletCountMin = 0;
        private float StartScanTime = 0;
        private const float ScanTimeMax = 3;

        // Update is called once per frame
        internal override Status Work()
        {
            switch (State)
            {
                case Status.Start:
                    if (Controller.EnemyIsSentry)
                    {
                        if (transform.position.x - Controller.EnemyPos.x < 0)
                            State = Status.EnemySentryComeFromTop;
                        else
                            State = Status.EnemySentryComeFromBottom;
                    }
                    else if (Controller.BeginTime > BeginTimeMax)
                        State = Status.Standby;
                    goto default;
                case Status.EnemySentryComeFromTop:
                    State = Status.ToCenter;
                    goto default;
                case Status.ToCenter:
                    State = Status.Standby;
                    goto default;
                case Status.Standby:
                    if (Controller.EnemyFind)
                        State = Status.Follow;
                    goto default;
                case Status.EnemySentryComeFromBottom:
                    State = Status.Peek;
                    goto default;
                case Status.Scan:
                case Status.Follow:
                    if (!Controller.EnemyFind)
                    {
                        if (State == Status.Follow)
                            StartScanTime = Controller.BeginTime;
                        State = Status.Scan;
                        if (Controller.BeginTime - StartScanTime > ScanTimeMax)
                            State = Status.Standby;
                    }
                    else if (Controller.EnemyFind)
                        State = Status.Follow;
                    if (Controller.AttckDirs.Count >= 2)
                        State = Status.Escape;
                    goto default;
                case Status.Escape:
                    if (Controller.AttckDirs.Count >= 2)
                        goto default;
                    else if (Controller.EnemyFind)
                        State = Status.Follow;
                    goto default;
                case Status.Supply:
                    if (Controller.Hp >= 600)
                        State = Status.Cruise;
                    else if (Controller.TotalResume >= 600)
                        State = Status.Cruise;
                    else
                        break;
                    goto default;
                case Status.Hide:
                    break;
                default:
                    if (Controller.BulletCount <= BulletCountMin)
                        State = Status.Hide;
                    else if (Controller.Hp < HpMin)
                    {
                        if (Controller.TotalResume >= 600)
                            State = Status.Hide;
                        else
                            State = Status.Supply;
                    }
                    break;
            }
            return State;
        }
    }
}
