using Distance.ReplayIntensifies.Scripts;
using HarmonyLib;

namespace Distance.ReplayIntensifies.Harmony
{
	[HarmonyPatch(typeof(ReplayManager), nameof(ReplayManager.OnEventModeStarted))]
	internal static class ReplayManager__OnEventModeStarted
	{
		[HarmonyPrefix]
		internal static void Prefix(ReplayManager __instance)
		{
			if (__instance.gm_.ModeID_ == GameModeID.LevelEditorPlay)
			{
				return;
			}

			if (Mod.Instance.Config.EnableRandomizedCars)
			{
				// Create and setup compound data for managing randomization that affects all replay cars.
				ReplayManagerCompoundData.Create(__instance);
			}
		}

		[HarmonyPostfix]
		internal static void Postfix(ReplayManager __instance)
		{
			// Cleanup for unused compound data.
			__instance.gameObject.RemoveComponent<ReplayManagerCompoundData>();
		}
	}
}
