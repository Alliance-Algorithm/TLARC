/// <summary>
/// 读取状态，得出行为模式
/// </summary>

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace RMUL
{
    class DecisionMaker_Universal : DecisionMaker
    {
        static Status State = Status.Start;

        private const float BeginTimeMax = 15;
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
                    if (Controller.BeginTime > BeginTimeMax)
                        State = Status.Standby;
                    goto default;
                case Status.Cruise:
                case Status.Standby:
                    if (Controller.EnemyFind)
                        State = Status.Follow;
                    else if (!Controller.GainPoint)
                        State = Status.Cruise;
                    else if (Controller.GainPoint)
                        State = Status.Standby;
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
                    if (!Controller.EnemyFind && c > 0)
                        State = Status.Escape;
                    break;
            }
            return State;
        }
    }
}
