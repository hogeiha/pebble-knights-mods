using System.Reflection;
using HarmonyLib;

namespace UniversalKingActions
{
    [HarmonyPatch]
    internal static class UpgradeCampAwakePatch
    {
        private static MethodBase TargetMethod()
        {
            return TypeFinder.FindMethod("UpgradeCamp", "Awake");
        }

        private static bool Prefix(object __instance)
        {
            UpgradeCampHider.Hide(__instance);
            return false;
        }
    }

    [HarmonyPatch]
    internal static class UpgradeCampInitializePatch
    {
        private static MethodBase TargetMethod()
        {
            return TypeFinder.FindMethod("UpgradeCamp", "Initialize");
        }

        private static bool Prefix(object __instance)
        {
            UpgradeCampHider.Hide(__instance);
            return false;
        }
    }
}
