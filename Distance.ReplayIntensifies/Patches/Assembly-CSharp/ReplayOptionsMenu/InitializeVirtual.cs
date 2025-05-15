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
	/// Patch the replay options menu slider to allow a maximum ghost count up to <see cref="Mod.MaxReplaysAtAll"/>.
	/// </summary>
	/// <remarks>
	/// Required For: Max Auto Replay Ghosts (part 2/2).
	/// </remarks>
	[HarmonyPatch(typeof(ReplayOptionsMenu), nameof(ReplayOptionsMenu.InitializeVirtual))]
	internal static class ReplayOptionsMenu__InitializeVirtual
	{
		// override void InitializeVirtual()
		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			Mod.Log.LogInfo("Transpiling...");
			// VISUAL:
			// Changes the max slider value parameter (20) to the new maximum specified by the mod.
			//__instance.TweakInt("GHOSTS IN ARCADE COUNT", __instance.settings_.GhostsInArcadeCount_, 1, 20, 5, delegate (int x)
			//{
			//	__instance.settings_.GhostsInArcadeCount_ = x;
			//}, null);

			var codes = new List<CodeInstruction>(instructions);
			for (int i = 5; i < codes.Count; i++)
			{
				// The only instance of 20 appearing as an operand in this function is for the max comparison.
				if ((codes[i - 5].opcode == OpCodes.Ldstr    && codes[i - 5].operand.ToString().ToUpperInvariant() == "GHOSTS IN ARCADE COUNT") &&
					(codes[i    ].opcode == OpCodes.Ldc_I4_S && codes[i    ].operand.ToString() == "20"))
				{
					Mod.Log.LogInfo($"ldc.i4.s 20 @ {i}");

					// Replace: ldc.i4.s 20
					// With:    ldc.i4 'Max(Mod.MaxReplaysAtAll, 20)'
					codes[i].opcode = OpCodes.Ldc_I4;
					codes[i].operand = (int)Math.Max(Mod.MaxReplaysAtAll, 20);

					break;
				}
			}
			return codes.AsEnumerable();
		}
	}
}
