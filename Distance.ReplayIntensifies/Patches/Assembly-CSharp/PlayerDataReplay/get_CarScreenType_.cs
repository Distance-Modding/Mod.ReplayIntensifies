using Distance.ReplayIntensifies.Scripts;
using HarmonyLib;

namespace Distance.ReplayIntensifies.Patches
{
	/// <summary>
	/// Patch to handle when to use the car screen depending on the car visual style.
	/// <para/>
	/// Separates handling of ghost mode and ghost visual states.
	/// </summary>
	/// <remarks>
	/// Required For: Car Visual Style (part 1/7).
	/// </remarks>
	[HarmonyPatch(typeof(PlayerDataReplay), nameof(PlayerDataReplay.CarScreenType_), MethodType.Getter)]
	internal static class PlayerDataReplay__get_CarScreenType_
	{
		[HarmonyPostfix]
		internal static void Postfix(PlayerDataReplay __instance, ref PlayerDataBase.CarScreenType __result)
		{
			//Mod.Log.LogDebug("get_CarScreenType_");
			var compoundData = __instance.GetComponent<PlayerDataReplayCompoundData>();
			if (compoundData)
			{
				__result = (!compoundData.IsGhostVisual) ? PlayerDataBase.CarScreenType.Normal : PlayerDataBase.CarScreenType.None;
			}
		}
	}
}
