using Distance.ReplayIntensifies.Scripts;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Distance.ReplayIntensifies.Patches
{
	/// <summary>
	/// Patch to setup all data tied to handling our extended car visual styles.
	/// <para/>
	/// Also includes patch to handle applying the car outline brightness.
	/// <para/>
	/// Separates handling of ghost mode and ghost visual states.
	/// </summary>
	/// <remarks>
	/// Required For: Car Visual Style (part 6/7).
	/// </remarks>
	[HarmonyPatch(typeof(PlayerDataReplay), nameof(PlayerDataReplay.InitPlayerDataReplay))]
	internal static class PlayerDataReplay__InitPlayerDataReplay
	{
		[HarmonyPrefix]
		internal static void Prefix(PlayerDataReplay __instance, CarReplayData data, bool isGhost)
		{
			// Setup our compound data and all associated info used to handle the car's visual style.
			var compoundData = PlayerDataReplayCompoundData.Create(__instance, data, isGhost);

			if (compoundData)
			{
				// Lazily ensure brightness is always updated in this method.
				__instance.outlineBrightness_ = compoundData.GetOutlineBrightness();
			}
		}

		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			Mod.Log.LogInfo("Transpiling...");
			// VISUAL:
			// Change skidmark removal to only occur for visual ghosts.
			// Replace outline brightness assignment with one determined by compound data.
			//if (this.isGhost_)
			// -to-
			//if (Mod.GetIsGhostVisual(this))
			//{
			//	this.outlineBrightness_ = this.replaySettings_.GhostBrightness_;
			//   -to-
			//  this.outlineBrightness_ = Mod.GetGhostBrightness(this);
			//
			//	this.skidmarkPrefab_ = null;
			//}

			var codes = new List<CodeInstruction>(instructions);
			for (int i = 5; i < codes.Count; i++)
			{
				if ((codes[i - 5].opcode == OpCodes.Ldfld    && ((FieldInfo) codes[i - 5].operand).Name == "isGhost_") &&
					(codes[i - 4].opcode == OpCodes.Brfalse) &&
					(codes[i - 1].opcode == OpCodes.Ldfld    && ((FieldInfo) codes[i - 1].operand).Name == "replaySettings_") &&
					(codes[i    ].opcode == OpCodes.Callvirt && ((MethodInfo)codes[i    ].operand).Name == "get_GhostBrightness_"))
				{
					Mod.Log.LogInfo($"ldfld isGhost_ @ {i-5}");
					Mod.Log.LogInfo($"callvirt get_GhostBrightness_ @ {i}");

					// Replace: ldfld isGhost_
					// With:    call Mod.GetIsGhostVisual
					codes[i - 5].opcode = OpCodes.Call;
					codes[i - 5].operand = typeof(Mod).GetMethod(nameof(Mod.GetIsGhostVisual));

					// Replace: ldfld replaySettings_
					// Replace: callvirt get_GhostBrightness_
					// With:    call Mod.GetGhostBrightness
					codes.RemoveRange(i - 1, 2);
					codes.InsertRange(i - 1, new CodeInstruction[]
					{
						new CodeInstruction(OpCodes.Call, typeof(Mod).GetMethod(nameof(Mod.GetGhostBrightness))),
					});

					break;
				}
			}
			return codes.AsEnumerable();
		}
	}
}
