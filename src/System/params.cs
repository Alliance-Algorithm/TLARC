#define humble
using Rcl;

namespace AllianceDM
{
    internal static class Ros2Def
    {
#if humble
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        internal static RclContext context;
        internal static IRclNode node;
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
#endif
    }
    internal static class DecisionMakerDef
    {
#if DEBUG
        internal static string ComponentsPath = "./Declaration/Component";
        internal static string GameObjectsPath = "./Declaration/Gameobject";
#else
        internal static string ComponentsPath = Environment.ProcessPath + "/../../../share/Declaration/Component";
        internal static string GameObjectsPath = Environment.ProcessPath + "/../../../share/Declaration/Gameobject";
#endif
        internal const int fps = 150;
    }

}