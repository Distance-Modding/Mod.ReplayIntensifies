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
	/// Required For: Car Visual Style (part 2/5).
	/// </remarks>
	[HarmonyPatch(typeof(PlayerDataReplay), nameof(PlayerDataReplay.GetLevelOfDetailType))]
	internal static class PlayerDataReplay__GetLevelOfDetailType
	{
		[HarmonyPostfix]
		internal static void Postfix(PlayerDataReplay __instance, ref CarLevelOfDetail.Type __result)
		{
			var compoundData = __instance.GetComponent<PlayerDataReplayCompoundData>();
			if (compoundData)
			{
				__result = compoundData.DetailType;
			}
		}
	}
}
