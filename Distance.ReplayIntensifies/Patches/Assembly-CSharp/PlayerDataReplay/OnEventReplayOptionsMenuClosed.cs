using Distance.ReplayIntensifies.Scripts;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Distance.ReplayIntensifies.Patches
{
	/// <summary>
	/// Patch to handle changes to the car outline brightness setting, and reflect those changes.
	/// <para/>
	/// Separates handling of ghost mode and ghost visual states.
	/// </summary>
	/// <remarks>
	/// Required For: Car Visual Style (part 7/7).
	/// </remarks>
	[HarmonyPatch(typeof(PlayerDataReplay), nameof(PlayerDataReplay.OnEventReplayOptionsMenuClosed))]
	internal static class PlayerDataReplay__OnEventReplayOptionsMenuClosed
	{
		[HarmonyPrefix]
		internal static void Prefix(PlayerDataReplay __instance)
		{
			var compoundData = __instance.GetComponent<PlayerDataReplayCompoundData>();
			if (compoundData)
			{
				// Lazily ensure brightness is always updated in this method.
				__instance.outlineBrightness_ = compoundData.GetOutlineBrightness();
			}
		}

		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var codes = new List<CodeInstruction>(instructions);

			Mod.Log.LogInfo("Transpiling... (1/2)");
			// VISUAL:
			// Replace outline brightness assignment with one determined by compound data.
			//if (this.isGhost_)
			//{
			//	this.outlineBrightness_ = this.replaySettings_.GhostBrightness_;
			//   -to-
			//  this.outlineBrightness_ = Mod.GetGhostBrightness(this);
			//}

			for (int i = 1; i < codes.Count; i++)
			{
				if ((codes[i - 1].opcode == OpCodes.Ldfld    && ((FieldInfo) codes[i - 1].operand).Name == "replaySettings_") &&
					(codes[i    ].opcode == OpCodes.Callvirt && ((MethodInfo)codes[i    ].operand).Name == "get_GhostBrightness_"))
				{
					Mod.Log.LogInfo($"callvirt get_GhostBrightness_ @ {i}");

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

			Mod.Log.LogInfo("Transpiling... (2/2)");
			// VISUAL:
			// Change jet flame/wing trail color updating to only occur for visual ghosts.
			//if (this.isGhost_)
			// -to-
			//if (Mod.GetIsGhostVisual(this))
			//{
			//	this.SetJetFlameColor();
			//	this.SetWingTrailColor(this.Car_);
			//}

			for (int i = 3; i < codes.Count; i++)
			{
				// Check for `call SetJetFlameColor` to ensure this is the correct instance of `ldfld isGhost_`.
				if ((codes[i - 3].opcode == OpCodes.Ldfld    && ((FieldInfo) codes[i - 3].operand).Name == "isGhost_") &&
					(codes[i - 2].opcode == OpCodes.Brfalse) &&
					(codes[i    ].opcode == OpCodes.Call     && ((MethodInfo)codes[i    ].operand).Name == "SetJetFlameColor"))
				{
					Mod.Log.LogInfo($"ldfld isGhost_ @ {i-3}");
					Mod.Log.LogInfo($"call SetJetFlameColor @ {i}");

					// Replace: ldfld isGhost_
					// With:    call Mod.GetIsGhostVisual
					codes[i - 3].opcode = OpCodes.Call;
					codes[i - 3].operand = typeof(Mod).GetMethod(nameof(Mod.GetIsGhostVisual));

					break;
				}
			}

			return codes.AsEnumerable();
		}
	}
}
