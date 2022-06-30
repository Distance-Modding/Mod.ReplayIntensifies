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
	/// Patch the replay playback(?) picker to allow a maximum ghost count up to Mod.MaxReplaysAtAll.
	/// <para/>
	/// It's important to patch this, and not just leave the maximum at <see cref="Mod.MaxReplaysAtAll"/>
	/// because otherwise the Select All function in the replay picker will have drastic consequences.
	/// </summary>
	[HarmonyPatch(typeof(ReplayManager), nameof(ReplayManager.PlayPickedReplays))]
	internal static class ReplayManager__PlayPickedReplays
	{
		// void PlayPickedReplays(string absoluteLevelPath, GameModeID gameModeID, IEnumerable<OnlineLeaderboard.Entry> entries, bool replayMode, bool advancedMenu)
		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			// VISUAL:
			// Changes the max count comparison operand (20) to the new maximum specified by the mod.
			//if (list.Count + __instance.pickedReplays_.transform.childCount >= 20)
			//{
			//	break;
			//}

			var codes = new List<CodeInstruction>(instructions);
			for (int i = 0; i < codes.Count; i++)
			{
				// The only instance of 20 appearing as an operand in this function is for the max comparison.
				if (codes[i].opcode == OpCodes.Ldc_I4_S && codes[i].operand.ToString() == "20")
				{

					// This wasn't working correctly because the transpiler was being run before
					//  `G.Sys.OptionsManager_.Replay_.GhostsInArcadeCount_` was fully initialized.
					//codes[i].opcode = OpCodes.Ldc_I4;
					//codes[i].operand = (int)Math.Max((int)Math.Min(Mod.MaxReplaysAtAll, G.Sys.OptionsManager_.Replay_.GhostsInArcadeCount_), 20);

					// Instead, we're falling back to the traditional static method call.

					// For some reason, using: CodeInstruction.Call(typeof(Mod), nameof(Mod.GetMaxReplays))
					//  prevents the Transpilers from loading. I think there's some assembly/type load order race
					//  shenannigans going on, so we're doing it the old fashioned way.
					codes[i].opcode = OpCodes.Call;
					codes[i].operand = typeof(Mod).GetMethod(nameof(Mod.GetMaxReplays));

					break;
				}
			}
			return codes.AsEnumerable();
		}


		// Prefix patch implementation:
		/*internal static bool Prefix(ReplayManager __instance, string absoluteLevelPath, GameModeID gameModeID, IEnumerable<OnlineLeaderboard.Entry> entries, bool replayMode, bool advancedMenu)
		{
			__instance.advancedMenu_ = advancedMenu;
			List<OnlineLeaderboard.ReplayRequest> list = new List<OnlineLeaderboard.ReplayRequest>();
			foreach (OnlineLeaderboard.Entry entry in entries)
			{
				int max = Math.Max(Math.Min(Mod.MaxReplaysAtAll, G.Sys.OptionsManager_.Replay_.GhostsInArcadeCount_), 20);
				if (list.Count + __instance.pickedReplays_.transform.childCount >= max)
				{
					break;
				}
				OfflineLeaderboardEntry offlineLeaderboardEntry = entry as OfflineLeaderboardEntry;
				if (offlineLeaderboardEntry != null)
				{
					CarReplayData carReplayData = CarReplayData.LoadFile(new PathAndFinishValue(offlineLeaderboardEntry.ReplayPath_, offlineLeaderboardEntry.Score_));
					if (carReplayData != null)
					{
						__instance.AddReplayToGroup(carReplayData, __instance.pickedReplays_);
					}
					else
					{
						Debug.LogError("PlayPickedReplays: Cant load replay: " + offlineLeaderboardEntry.ReplayPath_);
					}
				}
				else
				{
					list.Add(entry.CreateReplayRequest());
				}
			}
			if (list.Count + __instance.pickedReplays_.transform.childCount > 0)
			{
				__instance.isReplayModeNextScene_ = replayMode;
				if (list.Count > 0)
				{
					G.Sys.SteamworksManager_.UGC_.DownloadReplays(absoluteLevelPath, gameModeID, list.ToArray(), new SteamworksUGC.OnReplayDownloadFinished(__instance.OnPickedOnlineReplayDownloadFinished));
				}
				else
				{
					__instance.gm_.GoToCurrentLevel(GameManager.OpenOnMainMenuInit.Leaderboard, __instance.advancedMenu_);
				}
			}

			// Skip the original method. We've performed everything it would have.
			return false;
		}*/
	}
}
