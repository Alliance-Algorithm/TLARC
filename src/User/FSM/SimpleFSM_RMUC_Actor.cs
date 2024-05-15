
using AllianceDM.StdComponent;
using Rosidl.Messages.Builtin;

namespace AllianceDM.StateMechines
{
    public class SimpleFSM_RMUC_Action(uint uuid, uint[] revid, string[] args) : Component(uuid, revid, args)
    {
        Transform2D HiddenPos;
        Transform2D CurisePosMain;
        Transform2D CurisePos2;
        Transform2D ControlPos;
        Transform2D RechargeArea;
        SimpleFSM_RMUC fsm;
        Transform2D SentryPos;
        Transform2D SentryTargetRevisePos;
        Transform2D TargetPos;
        float timer;
        bool comeback;
        float maxtime;
        public override void Awake()
        {
            ControlPos = DecisionMaker.FindComponent<Transform2D>(RecieveID[0]);
            CurisePosMain = DecisionMaker.FindComponent<Transform2D>(RecieveID[1]);
            CurisePos2 = DecisionMaker.FindComponent<Transform2D>(RecieveID[2]);
            HiddenPos = DecisionMaker.FindComponent<Transform2D>(RecieveID[3]);
            RechargeArea = DecisionMaker.FindComponent<Transform2D>(RecieveID[4]);
            fsm = DecisionMaker.FindComponent<SimpleFSM_RMUC>(RecieveID[5]);
            SentryPos = DecisionMaker.FindComponent<Transform2D>(RecieveID[6]);
            SentryTargetRevisePos = DecisionMaker.FindComponent<Transform2D>(RecieveID[7]);
            TargetPos = DecisionMaker.FindComponent<Transform2D>(uint.Parse(Args[0]));
            maxtime = float.Parse(Args[1]);
        }
        public override void Update()
        {
            comeback = false;
            switch (fsm.Output)
            {
                case Status.Invinciable:
                    timer = DateTime.Now.Second;
                    TargetPos.Set(ControlPos.Output.pos);
                    break;
                case Status.LowState:
                    if (DateTime.Now.Second - timer > maxtime - 15)
                        comeback = true;
                    if (comeback)
                    {
                        TargetPos.Set(CurisePosMain.Output.pos);
                        if ((SentryPos.Output.pos - CurisePosMain.Output.pos +
                         SentryTargetRevisePos.Output.pos).Length() < 0.1f)
                            timer = DateTime.Now.Second;
                    }
                    else
                        TargetPos.Set(RechargeArea.Output.pos);
                    break;
                case Status.Curise:
                    timer = DateTime.Now.Second;
                    TargetPos.Set(CurisePosMain.Output.pos);
                    break;
                case Status.Hidden:
                    if (DateTime.Now.Second - timer > maxtime - 10)
                        comeback = true;
                    if (comeback)
                    {
                        TargetPos.Set(CurisePosMain.Output.pos);
                        if ((SentryPos.Output.pos - CurisePosMain.Output.pos +
                         SentryTargetRevisePos.Output.pos).Length() < 0.1f)
                            timer = DateTime.Now.Second;
                    }
                    else
                        TargetPos.Set(HiddenPos.Output.pos);
                    break;
            }


        }
    }
}