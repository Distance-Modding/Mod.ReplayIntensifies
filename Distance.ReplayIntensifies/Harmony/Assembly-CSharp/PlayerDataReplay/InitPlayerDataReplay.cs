using Distance.ReplayIntensifies.Scripts;
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
	/// Patch to change the value of <see cref="PlayerDataReplay.simulateNetworkCar_"/> for the duration of the function.
	/// <para/>
	/// Also includes patch to handle applying the car outline brightness.
	/// </summary>
	/// <remarks>
	/// Required For: Car Visual Style (part 4/5).
	/// </remarks>
	[HarmonyPatch(typeof(PlayerDataReplay), nameof(PlayerDataReplay.InitPlayerDataReplay))]
	internal static class PlayerDataReplay__InitPlayerDataReplay
	{
		[HarmonyPrefix]
		internal static void Prefix(PlayerDataReplay __instance, out bool? __state, CarReplayData data, bool isGhost)
		{
			__state = null;

			var compoundData = __instance.GetComponent<PlayerDataReplayCompoundData>();
			if (compoundData)
			{
				// Backup then change `simulateNetworkCar_` state.
				__state = PlayerDataReplay.simulateNetworkCar_;
				PlayerDataReplay.simulateNetworkCar_ = compoundData.SimulateNetworkCar;

				// Brightness only updated for ghosts in original method, so update it here for non-ghosts.
				if (!__instance.IsGhost_)
				{
					__instance.outlineBrightness_ = compoundData.GetOutlineBrightness();
				}
			}
		}

		[HarmonyPostfix]
		internal static void Postfix(bool? __state)
		{
			// Restore original `simulateNetworkCar_` state.
			if (__state.HasValue)
			{
				PlayerDataReplay.simulateNetworkCar_ = __state.Value;
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
			//	this.skidmarkPrefab_ = null;
			//   -to-
			//  this.outlineBrightness_ = GetGhostBrightness_(this);
			//	this.skidmarkPrefab_ = null;
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
						new CodeInstruction(OpCodes.Call, typeof(PlayerDataReplay__InitPlayerDataReplay).GetMethod(nameof(GetGhostBrightness_))),
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
