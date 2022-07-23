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
	/// Additionally calls <see cref="Mod.CreateCarOutline(PlayerDataReplay)"/> for Replay-style car rendering.
	/// </summary>
	/// <remarks>
	/// Required For: Car Visual Style (part 3/5).
	/// </remarks>
	[HarmonyPatch(typeof(PlayerDataReplay), nameof(PlayerDataReplay.InitCarBlueprintVirtual))]
	internal static class PlayerDataReplay__InitCarBlueprintVirtual
	{
		[HarmonyPrefix]
		internal static void Prefix(PlayerDataReplay __instance, out bool? __state, GameObject carBlueprint)
		{
			__state = null;

			var compoundData = __instance.GetComponent<PlayerDataReplayCompoundData>();
			if (compoundData)
			{
				// Backup then change `simulateNetworkCar_` state.
				__state = PlayerDataReplay.simulateNetworkCar_;
				PlayerDataReplay.simulateNetworkCar_ = compoundData.SimulateNetworkCar;
				if (compoundData.DetailType == CarLevelOfDetail.Type.Replay)
				{
					// Only case where `CreateCarOutline` wouldn't be called by the original function.
					Mod.CreateCarOutline(__instance);
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
			// Replace calls to `this.CreateCarOutline()` with `Mod.CreateCarOutline(this)`.
			//if (this.isGhost_)
			//{
			//	UnityEngine.Object.DestroyImmediate(carBlueprint.GetComponent<DataEffect>());
			//	UnityEngine.Object.DestroyImmediate(carBlueprint.GetComponent<DataEffectLogic>());
			//	this.CreateCarOutline();
			//}
			//else
			//{
			//	if (PlayerDataReplay.simulateNetworkCar_)
			//	{
			//		this.CreateCarOutline();
			//	}
			//	carBlueprint.GetComponent<DataEffect>().InitBlueprint();
			//}

			var codes = new List<CodeInstruction>(instructions);
			for (int i = 0; i < codes.Count; i++)
			{
				if (codes[i].opcode == OpCodes.Call && ((MethodInfo)codes[i].operand).Name == "CreateCarOutline")
				{
					Mod.Instance.Logger.Info($"call CreateCarOutline @ {i}");

					// Replace: call CreateCarOutline
					// With:    call Mod.CreateReplayCarOutline
					codes[i].opcode = OpCodes.Call;
					codes[i].operand = typeof(Mod).GetMethod(nameof(Mod.CreateCarOutline));

					// This instruction appears 2 times, so no breaking after our first find.
				}
			}
			return codes.AsEnumerable();
		}
	}
}
