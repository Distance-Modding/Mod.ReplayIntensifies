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
	/// Patch the replay car spawner to allow a maximum ghost count up to <see cref="Mod.MaxReplaysAtAll"/>.
	/// </summary>
	/// <remarks>
	/// Required For: Max Auto Replay Ghosts (part 1/2), and Max Selected Replay Ghosts (part 2/2).
	/// </remarks>
	[HarmonyPatch(typeof(ReplayManager), nameof(ReplayManager.SpawnReplay))]
	internal static class ReplayManager__SpawnReplay
	{
		// void SpawnReplay(CarReplayData replayData, bool isGhost)
		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			Mod.Log.LogInfo("Transpiling...");
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
					Mod.Log.LogInfo($"ldc.i4.s 20 @ {i}");

					// Replace: ldc.i4.s 20
					// With:    call Mod.GetMaxSpawnReplays
					codes[i].opcode = OpCodes.Call;
					codes[i].operand = typeof(Mod).GetMethod(nameof(Mod.GetMaxSpawnReplays));

					break;
				}
			}
			return codes.AsEnumerable();
		}
	}
}
