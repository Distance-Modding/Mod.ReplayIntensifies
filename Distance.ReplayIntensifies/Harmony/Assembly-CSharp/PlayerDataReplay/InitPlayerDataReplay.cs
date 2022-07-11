using Distance.ReplayIntensifies.Scripts;
using HarmonyLib;
using System;
using UnityEngine;

namespace Distance.ReplayIntensifies.Harmony
{
	/// <summary>
	/// Patch to change the value of <see cref="PlayerDataReplay.simulateNetworkCar_"/> for the duration of the function.
	/// </summary>
	/// <remarks>
	/// Required For: Car Visual Style (part 4/4).
	/// </remarks>
	[HarmonyPatch(typeof(PlayerDataReplay), nameof(PlayerDataReplay.InitPlayerDataReplay))]
	internal static class PlayerDataReplay__InitPlayerDataReplay
	{
		[HarmonyPrefix]
		internal static void Prefix(PlayerDataReplay __instance, out bool? __state, CarReplayData data, bool isGhost)
		{
			__state = null;

			var compoundData = __instance.GetComponent<PlayerDataReplayCompoundData>();

			{
				// Backup then change `simulateNetworkCar_` state.
				__state = PlayerDataReplay.simulateNetworkCar_;
				PlayerDataReplay.simulateNetworkCar_ = compoundData.SimulateNetworkCar;
			}
		}

		[HarmonyPostfix]
		internal static void Postfix(bool? __state)
		{
			// Restore original `simulateNetworkCar_` state.
			if (__state.HasValue)
			{
				PlayerDataReplay.simulateNetworkCar_ = __state.Value;
			}
		}
	}
}
