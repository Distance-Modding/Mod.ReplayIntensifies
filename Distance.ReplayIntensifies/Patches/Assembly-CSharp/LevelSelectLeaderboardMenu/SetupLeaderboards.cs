using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace Distance.ReplayIntensifies.Patches
{
	/// <summary>
	/// Patch to change the maximum number of online leaderboards entries that can be downloaded.
	/// </summary>
	/// <remarks>
	/// Required For: Max Online Leaderboards (part 2/2).
	/// </remarks>
	[HarmonyPatch(typeof(LevelSelectLeaderboardMenu), nameof(LevelSelectLeaderboardMenu.SetupLeaderboards))]
	internal static class LevelSelectLeaderboardMenu__SetupLeaderboards
	{
		// void SetupLeaderboards()
		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			Mod.Log.LogInfo("Transpiling...");
			// VISUAL:
			// Changes the max entries count argument (1000) to the new maximum specified by the mod.
			//G.Sys.SteamworksManager_.Leaderboard_.DownloadLeaderboardInfo(selectedLevelPath_, this.modeInfo_.ID,
			//		new SteamworksLeaderboard.OnLeaderboardDownloaded(this.OnSteamLeaderboardDownloadedGlobal),
			//		OnlineLeaderboard.RangeRequestType.Global, 1, 1000, false);
			// -and-
			//G.Sys.SteamworksManager_.Leaderboard_.DownloadLeaderboardInfo(selectedLevelPath_, this.modeInfo_.ID,
			//		new SteamworksLeaderboard.OnLeaderboardDownloaded(this.OnSteamLeaderboardDownloadedFriend),
			//		OnlineLeaderboard.RangeRequestType.Friends, 1, 1000, false);

			var codes = new List<CodeInstruction>(instructions);
			for (int i = 0; i < codes.Count; i++)
			{
				// The only instances of 1000 appearing as an operand in this function is for the max argument.
				if (codes[i].opcode == OpCodes.Ldc_I4 && codes[i].operand.ToString() == "1000")
				{
					Mod.Log.LogInfo($"ldc.i4 1000 @ {i}");

					// Replace: ldc.i4 1000
					// With:    call Mod.GetMaxOnlineLeaderboards
					codes[i].opcode = OpCodes.Call;
					codes[i].operand = typeof(Mod).GetMethod(nameof(Mod.GetMaxOnlineLeaderboards));

					// This instruction appears 2 times, so no breaking after our first find.
				}
			}
			return codes.AsEnumerable();
		}
	}
}
