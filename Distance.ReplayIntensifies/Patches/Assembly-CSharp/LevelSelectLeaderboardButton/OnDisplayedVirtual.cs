using HarmonyLib;

namespace Distance.ReplayIntensifies.Patches
{
	/// <summary>
	/// Patch to highlight rivals in the level select leaderboards menu.
	/// </summary>
	/// <remarks>
	/// Required For: Highlight Rivals in Leaderboards
	/// </remarks>
	[HarmonyPatch(typeof(LevelSelectLeaderboardButton), nameof(LevelSelectLeaderboardButton.OnDisplayedVirtual))]
	internal static class LevelSelectLeaderboardButton__OnDisplayedVirtual
	{
		[HarmonyPostfix]
		internal static void Postfix(LevelSelectLeaderboardButton __instance)
		{
			Mod.UpdateLeaderboardButtonColor(__instance, false);
		}
	}
}
