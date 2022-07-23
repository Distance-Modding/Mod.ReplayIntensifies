using Distance.ReplayIntensifies.Scripts;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Distance.ReplayIntensifies.Harmony
{
	/// <summary>
	/// Patch to handle changes to the car outline brightness setting, and reflect those changes.
	/// </summary>
	/// <remarks>
	/// Required For: Car Visual Style (part 5/5).
	/// </remarks>
	[HarmonyPatch(typeof(PlayerDataReplay), nameof(PlayerDataReplay.OnEventReplayOptionsMenuClosed))]
	internal static class PlayerDataReplay__OnEventReplayOptionsMenuClosed
	{
		[HarmonyPrefix]
		internal static void Prefix(PlayerDataReplay __instance)
		{
			// Brightness only updated for ghosts in original method, so update it here for non-ghosts.
			var compoundData = __instance.GetComponent<PlayerDataReplayCompoundData>();
			if (compoundData && !__instance.IsGhost_)
			{
				__instance.outlineBrightness_ = compoundData.GetOutlineBrightness();
			}
		}

		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			Mod.Instance.Logger.Info("Transpiling...");
			// VISUAL:
			// Replace outline brightness assignment with one determined by compound data.
			//if (this.isGhost_)
			//{
			//	this.outlineBrightness_ = this.replaySettings_.GhostBrightness_;
			//   -to-
			//  this.outlineBrightness_ = GetGhostBrightness_(this);
			//}

			var codes = new List<CodeInstruction>(instructions);
			for (int i = 1; i < codes.Count; i++)
			{
				if ((codes[i - 1].opcode == OpCodes.Ldfld    && ((FieldInfo) codes[i - 1].operand).Name == "replaySettings_") &&
					(codes[i    ].opcode == OpCodes.Callvirt && ((MethodInfo)codes[i    ].operand).Name == "get_GhostBrightness_"))
				{
					Mod.Instance.Logger.Info($"callvirt get_GhostBrightness_ @ {i}");

					// Replace: ldfld replaySettings_
					// Replace: callvirt get_GhostBrightness_
					// With:    call GetGhostBrightness_
					codes.RemoveRange(i - 1, 2);
					codes.InsertRange(i - 1, new CodeInstruction[]
					{
						new CodeInstruction(OpCodes.Call, typeof(PlayerDataReplay__OnEventReplayOptionsMenuClosed).GetMethod(nameof(GetGhostBrightness_))),
					});

					break;
				}
			}
			return codes.AsEnumerable();
		}

		#region Helper Functions

		// Trailing underscore added since there's no HarmonyIgnore attribute.
		// Override for determining ghost brightness level.
		public static float GetGhostBrightness_(PlayerDataReplay playerDataReplay)
		{
			var compoundData = playerDataReplay.GetComponent<PlayerDataReplayCompoundData>();
			if (compoundData)
			{
				return compoundData.GetOutlineBrightness();
			}
			else
			{
				return playerDataReplay.replaySettings_.GhostBrightness_;
			}
		}

		#endregion
	}
}
