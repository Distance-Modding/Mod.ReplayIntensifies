using HarmonyLib;
using UnityEngine;

namespace Distance.ReplayIntensifies.Harmony
{
	/// <summary>
	/// Patch to prevent switching to cinematic cameras in replay mode.
	/// </summary>
	/// <remarks>
	/// Required For: Disable Cinematic Cameras in Replay Mode.
	/// </remarks>
	[HarmonyPatch(typeof(CinematicCameraMode), nameof(CinematicCameraMode.SwitchToCamera), typeof(CinematicCamera), typeof(Transform), typeof(float), typeof(float), typeof(float), typeof(float))]
	internal static class CinematicCameraMode__SwitchToCamera
	{
		[HarmonyPrefix]
		internal static bool Prefix(CinematicCameraMode __instance)
		{
			if (G.Sys.ReplayManager_.IsReplayMode_ && Mod.Instance.Config.ReplayModeDisableCinematicCameras)
			{
				return false; // Don't switch to cinematic camera.
			}

			return true; // Switch during normal gameplay or if the setting is disabled.
		}
	}
}
