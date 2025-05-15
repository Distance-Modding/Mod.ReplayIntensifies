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
	/// Patch the replay playback(?) picker to allow a maximum ghost count up to Mod.MaxReplaysAtAll.
	/// <para/>
	/// It's important to patch this, and not just leave the maximum at <see cref="Mod.MaxReplaysAtAll"/>
	/// because otherwise the Select All function in the replay picker will have drastic consequences.
	/// </summary>
	/// <remarks>
	/// Required For: Max Selected Replay Ghosts (part 1/2).
	/// </remarks>
	[HarmonyPatch(typeof(ReplayManager), nameof(ReplayManager.PlayPickedReplays))]
	internal static class ReplayManager__PlayPickedReplays
	{
		// void PlayPickedReplays(string absoluteLevelPath, GameModeID gameModeID, IEnumerable<OnlineLeaderboard.Entry> entries, bool replayMode, bool advancedMenu)
		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			Mod.Log.LogInfo("Transpiling...");
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
					Mod.Log.LogInfo($"ldc.i4.s 20 @ {i}");

					// Replace: ldc.i4.s 20
					// With:    call Mod.GetMaxPickedReplays
					codes[i].opcode = OpCodes.Call;
					codes[i].operand = typeof(Mod).GetMethod(nameof(Mod.GetMaxPickedReplays));
					/*codes.RemoveAt(i);

					codes.InsertRange(i, new CodeInstruction[]
					{
						new CodeInstruction(OpCodes.Ldarg_S, (byte)4),
						new CodeInstruction(OpCodes.Call, typeof(Mod).GetMethod(nameof(Mod.GetMaxPickedReplays))),
					});*/

					break;
				}
			}
			return codes.AsEnumerable();
		}
	}
}
