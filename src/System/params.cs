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
                internal static string ComponentsPath = "./declarations/";
#else
        internal static string ComponentsPath = Environment.ProcessPath + "/../../../share/tlarc/declarations/";
#endif
                internal const int fps = 10;
        }

}