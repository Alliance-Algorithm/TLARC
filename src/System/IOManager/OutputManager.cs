using System;
using AllianceDM;
namespace AllianceDM.IO
{
    class OutputManager : Component
    {
        public OutputManager(uint uuid, uint[] recvid, string[] args) : base(uuid, recvid, args)
        {

        }
        public override void Awake()
        {
            for (int i = 0; i < Args.Length; i += 2)
            {
                DecisionMaker.FindComponent<Component>(RecieveID[i / 2]).Echo(Args[i], int.Parse(s: Args[i + 1]));
            }
        }
    }
}