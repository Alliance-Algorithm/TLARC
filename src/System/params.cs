#define humble
using Rcl;

namespace AllianceDM
{
        internal static class Ros2Def
        {
#if humble
                internal static RclContext context;
                internal static IRclNode node;
#endif
        }
        internal static class DecisionMakerDef
        {
#if DEBUG
                internal static string ComponentsPath = "./Declaration/Component";
                internal static string GameObjectsPath = "./Declaration/Gameobject";
#else
        internal static string ComponentsPath = Environment.ProcessPath + "/../../../share/decision_maker/Declaration/Component";
        internal static string GameObjectsPath = Environment.ProcessPath + "/../../../share/decision_maker/Declaration/Gameobject";
#endif
                internal const int fps = 10;
        }

}