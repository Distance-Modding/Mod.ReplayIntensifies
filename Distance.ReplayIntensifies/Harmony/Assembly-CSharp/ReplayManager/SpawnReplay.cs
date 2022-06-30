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
	/// Patch the replay car spawner to allow a maximum ghost count up to <see cref="Mod.MaxReplaysAtAll"/>.
	/// </summary>
	[HarmonyPatch(typeof(ReplayManager), nameof(ReplayManager.SpawnReplay))]
	internal static class ReplayManager__SpawnReplay
	{
		// void SpawnReplay(CarReplayData replayData, bool isGhost)
		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			// VISUAL:
			// Changes the max count comparison operand (20) to the new maximum specified by the mod.
			//if (PlayerDataReplay.ReplayPlayers_.Count >= 20 || !ReplayManager.SaveLoadReplays_)
			//{
			//	return false;
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
		/*internal static bool Prefix(ReplayManager __instance, CarReplayData replayData, bool isGhost)
		{
			int max = Math.Max(Math.Min(Mod.MaxReplaysAtAll, G.Sys.OptionsManager_.Replay_.GhostsInArcadeCount_), 20);
			if (PlayerDataReplay.ReplayPlayers_.Count >= max || !ReplayManager.SaveLoadReplays_)
			{
				// Skip the original method. We've performed everything it would have.
				return false;
			}
			if (replayData.enabled)
			{
				PlayerDataReplay playerDataReplay = PlayerDataReplay.CreatePlayerDataReplay(__instance.playerDataPrefab_, replayData, isGhost);
				if (playerDataReplay)
				{
					__instance.longestReplayLength_ = Mathf.Max(__instance.longestReplayLength_, (float) playerDataReplay.CarReplayData_.ReplayLengthMS_ / 1000f);
				}
			}

			// Skip the original method. We've performed everything it would have.
			return false;
		}*/
	}
}
