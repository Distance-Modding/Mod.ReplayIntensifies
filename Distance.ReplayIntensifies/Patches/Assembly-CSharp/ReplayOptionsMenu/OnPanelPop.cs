using HarmonyLib;

namespace Distance.ReplayIntensifies.Patches
{
    [HarmonyPatch(typeof(ReplayOptionsMenu), nameof(ReplayOptionsMenu.OnPanelPop))]
    internal static class ReplayOptionsMenu__OnPanelPop
    {
        [HarmonyPostfix]
        internal static void Postfix(ReplayOptionsMenu __instance)
        {
            Mod.MaxAutoReplays.Value = __instance.settings_.GhostsInArcadeCount_;
        }
    }
}
