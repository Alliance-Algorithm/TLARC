using System.Numerics;
using AllianceDM.StdComponent;

namespace AllianceDM.Nav
{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
    public class Astar(uint uuid, uint[] revid, string[] args) : LocalPathFinder(uuid, revid, args)
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
    {
        GlobalPathFinder nav;
        Transform2D sentry;

        public override void Awake()
        {
            nav = DecisionMaker.FindComponent<GlobalPathFinder>(RecieveID[0]);
            sentry = DecisionMaker.FindComponent<Transform2D>(RecieveID[1]);
        }
        public override void Update()
        {
            Dir = nav.Output - sentry.Output.pos;
        }
    }
}