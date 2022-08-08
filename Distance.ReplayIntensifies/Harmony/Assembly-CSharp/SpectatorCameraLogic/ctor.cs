using HarmonyLib;
using System;

namespace Distance.ReplayIntensifies.Harmony
{
	/// <summary>
	/// Patch to handle changing the pre-spectate timer delay (after finishing before entering semi-spectate mode).
	/// </summary>
	/// <remarks>
	/// Required For: Change Pre-Spectate Timer.
	/// </remarks>
	[HarmonyPatch(typeof(SpectatorCameraLogic), MethodType.Constructor)]
	internal static class SpectatorCameraLogic__ctor
	{
		[HarmonyPostfix]
		internal static void Postfix(SpectatorCameraLogic __instance)
		{
			// Have a minimum time of 1 frame to avoid any potential jank that Distance isn't prepared for.
			__instance.preSpectateTimer_ = Math.Max(Mod.Instance.Config.FinishPreSpectateTime, 1f / 60f);
		}
	}
}