using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace Distance.ReplayIntensifies.Harmony
{
	/// <summary>
	/// Patch to change the maximum number of online leaderboards entries that can be downloaded.
	/// </summary>
	/// <remarks>
	/// Required For: Max Online Leaderboards (part 1/2).
	/// </remarks>
	[HarmonyPatch(typeof(FinishMenuLogic), nameof(FinishMenuLogic.ShowOnlineLeaderboards))]
	internal static class FinishMenuLogic__SetupLeaderboards
	{
		// void ShowOnlineLeaderboards(OnlineLeaderboard.RangeRequestType requestType)
		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			Mod.Instance.Logger.Info("Transpiling...");
			// VISUAL:
			// Changes the max entries count argument (1000) to the new maximum specified by the mod.
			//this.steamworks_.Leaderboard_.DownloadLeaderboardInfo(this.gameManager_.LevelPath_, this.gameManager_.Mode_.GameModeID_,
			//		new SteamworksLeaderboard.OnLeaderboardDownloaded(this.OnSteamLeaderboardDownloaded),
			//		requestType, 1, 1000, false);

			var codes = new List<CodeInstruction>(instructions);
			for (int i = 0; i < codes.Count; i++)
			{
				// The only instance of 1000 appearing as an operand in this function is for the max entries argument.
				if (codes[i].opcode == OpCodes.Ldc_I4 && codes[i].operand.ToString() == "1000")
				{
					Mod.Instance.Logger.Info($"ldc.i4 1000 @ {i}");

					// Replace: ldc.i4 1000
					// With:    call Mod.GetMaxOnlineLeaderboards
					codes[i].opcode = OpCodes.Call;
					codes[i].operand = typeof(Mod).GetMethod(nameof(Mod.GetMaxOnlineLeaderboards));

					break;
				}
			}
			return codes.AsEnumerable();
		}
	}
}
