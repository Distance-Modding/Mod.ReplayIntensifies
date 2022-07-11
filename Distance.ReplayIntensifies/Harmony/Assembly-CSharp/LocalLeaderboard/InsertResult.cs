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
	/// Patch to extend limit for saved local replays, by allowing replays at higher indexes to be inserted.
	/// </summary>
	/// <remarks>
	/// Required For: Max Saved Local Replays (part 1/2).
	/// </remarks>
	[HarmonyPatch(typeof(LocalLeaderboard), nameof(LocalLeaderboard.InsertResult))]
	internal static class LocalLeaderboard__InsertResult
	{
		// bool InsertResult(string profileName, int profileID, int value, CarReplayData.GUID replayGuid)
		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			Mod.Instance.Logger.Info("Transpiling...");
			// VISUAL:
			// Changes the max count comparison operand (20) to the new maximum specified by the mod.
			//if (newEntryIndex < 20)
			//{
			//	this.results_.Insert(newEntryIndex, new ResultInfo(profileName, profileID, value, replayGuid));
			//	this.TrimResults();
			//	return true;
			//}

			var codes = new List<CodeInstruction>(instructions);
			for (int i = 0; i < codes.Count; i++)
			{
				// The only instance of 20 appearing as an operand in this function is for the max comparison.
				if (codes[i].opcode == OpCodes.Ldc_I4_S && codes[i].operand.ToString() == "20")
				{
					Mod.Instance.Logger.Info($"ldc.i4.s 20 @ {i}");

					// Replace: ldc.i4.s 20
					// With:    call Mod.GetMaxSavedLocalReplays
					codes[i].opcode = OpCodes.Call;
					codes[i].operand = typeof(Mod).GetMethod(nameof(Mod.GetMaxSavedLocalReplays));

					break;
				}
			}
			return codes.AsEnumerable();
		}
	}
}
