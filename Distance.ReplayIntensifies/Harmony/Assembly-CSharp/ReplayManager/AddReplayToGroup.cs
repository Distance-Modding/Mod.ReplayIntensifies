using Distance.ReplayIntensifies.Scripts;
using HarmonyLib;
using UnityEngine;

namespace Distance.ReplayIntensifies.Harmony
{
	/// <summary>
	/// Patch to mark replays that did not finish the level, and ensure compound data is always created.
	/// </summary>
	/// <remarks>
	/// Required For: Randomized Cars.
	/// </remarks>
	[HarmonyPatch(typeof(ReplayManager), nameof(ReplayManager.AddReplayToGroup))]
	internal static class ReplayManager__AddReplayToGroup
	{
		[HarmonyPostfix]
		internal static void Postfix(ReplayManager __instance, CarReplayData carReplayData, GameObject group)
		{
			if (carReplayData)
			{
				var carCompoundData = carReplayData.gameObject.GetOrAddComponent<CarReplayDataCompoundData>();
				if (carCompoundData)
				{
					carCompoundData.DidNotFinish = (group == __instance.didNotFinishReplays_);
				}
			}
		}
	}
}
