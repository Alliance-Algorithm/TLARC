using UnityEngine;

namespace RMUL
{
    public abstract class DecisionMaker : MonoBehaviour
    {
        internal enum Status
        {
            //-------------unversal---------------->
            Start,//初始状态
            Cruise,//巡航
            Standby,//原地待命
            Follow,//跟随
            Escape,//逃跑S
            Hide,//躲藏
            Supply,//补给
            Scan,//follow跟丢后的扫描

            //-------------unversal----------------<

            //-------------advantageous------------>
            AttackBase,//攻击基地

            //-------------advantageous------------<

            //-------------disadvantageous--------->
            EnemySentryComeFromTop,//敌人哨兵从上方来
            EnemySentryComeFromBottom,//敌人哨兵从下方来
            ToCenter,//前往中心增益点
            Peek,//窥探
            //-------------disadvantageous---------<
        }

        internal abstract Status Work();
    }
}