/// <summary>
/// 打弱势队伍的决策树(己方优势)
/// </summary>

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace RMUL
{
    class DecisionMaker_Advantageous : DecisionMaker
    {
        static Status State = Status.Start;

        private const float BeginTimeMax = 20;
        private const float HpMin = 100;
        private const float BulletCountMin = 0;
        private float StartScanTime = 0;
        private const float ScanTimeMax = 3;

        // Update is called once per frame
        internal override Status Work()
        {
            int c = 0;
            foreach (var i in Controller.AttackDirs)
                c += i > 0 ? 1 : 0;
            switch (State)
            {
                case Status.Start:
                    State = Status.Standby;
                    goto default;
                case Status.Standby:
                    if (Controller.EnemySentryDead)
                        State = Status.AttackBase;
                    else if (Controller.EnemyFind)
                        State = Status.Follow;
                    goto default;
                case Status.AttackBase:
                    goto default;
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
                    if (c >= 2)
                        State = Status.Escape;
                    goto default;
                case Status.Escape:
                    if (c >= 2)
                        goto default;
                    else if (Controller.EnemyFind)
                        State = Status.Follow;
                    goto default;
                case Status.Supply:
                    if (Controller.Hp >= 600)
                        State = Status.Standby;
                    else if (Controller.TotalResume >= 600)
                        State = Status.Standby;
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
