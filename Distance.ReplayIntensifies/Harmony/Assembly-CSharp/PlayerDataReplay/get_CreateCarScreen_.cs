using Distance.ReplayIntensifies.Scripts;
using HarmonyLib;

namespace Distance.ReplayIntensifies.Harmony
{
	/// <summary>
	/// Patch to handle when to use the car screen depending on the car visual style.
	/// <para/>
	/// Separates handling of ghost mode and ghost visual states.
	/// </summary>
	/// <remarks>
	/// Required For: Car Visual Style (part 2/7).
	/// </remarks>
	[HarmonyPatch(typeof(PlayerDataReplay), nameof(PlayerDataReplay.CreateCarScreen_), MethodType.Getter)]
	internal static class PlayerDataReplay__get_CreateCarScreen_
	{
		[HarmonyPostfix]
		internal static void Postfix(PlayerDataReplay __instance, ref bool __result)
		{
			//Mod.Instance.Logger.Debug("get_CreateCarScreen_");
			var compoundData = __instance.GetComponent<PlayerDataReplayCompoundData>();
			if (compoundData)
			{
				__result = !compoundData.IsGhostVisual;
			}
		}
	}
}
