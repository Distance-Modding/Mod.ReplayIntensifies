using Distance.ReplayIntensifies.Scripts;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace Distance.ReplayIntensifies.Harmony
{
	/// <summary>
	/// Patch to change the handling for creation of the car outline.
	/// <para/>
	/// Also includes patch to determine whether to use the data materialization spawn effect for cars.
	/// </summary>
	/// <remarks>
	/// Required For: Car Visual Style (part 4/7).
	/// </remarks>
	[HarmonyPatch(typeof(PlayerDataReplay), nameof(PlayerDataReplay.InitCarBlueprintVirtual))]
	internal static class PlayerDataReplay__InitCarBlueprintVirtual
	{
		[HarmonyPrefix]
		internal static void Prefix(PlayerDataReplay __instance, GameObject carBlueprint)
		{
			var compoundData = __instance.GetComponent<PlayerDataReplayCompoundData>();
			if (compoundData && !Mod.GetDontShowDataEffect(__instance) && !PlayerDataReplay.simulateNetworkCar_)
			{
				// Only case where `CreateCarOutline` wouldn't be called by the original function.
				Mod.CreateCarOutline(__instance);
			}
		}

		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var codes = new List<CodeInstruction>(instructions);

			Mod.Instance.Logger.Info("Transpiling... (1/2)");
			// VISUAL:
			// Replace car outline creation with a function where creation is determined by compound data.
			//if (this.isGhost_)
			//{
			//	UnityEngine.Object.DestroyImmediate(carBlueprint.GetComponent<DataEffect>());
			//	UnityEngine.Object.DestroyImmediate(carBlueprint.GetComponent<DataEffectLogic>());
			//
			//	this.CreateCarOutline();
			//	 -to-
			//	Mod.CreateCarOutline(this);
			//}
			//else
			//{
			//	if (PlayerDataReplay.simulateNetworkCar_)
			//	{
			//		this.CreateCarOutline();
			//		 -to-
			//		Mod.CreateCarOutline(this);
			//	}
			//	carBlueprint.GetComponent<DataEffect>().InitBlueprint();
			//}

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

			Mod.Instance.Logger.Info("Transpiling... (2/2)");
			// VISUAL:
			//if (this.isGhost_)
			// -to-
			//if (Mod.GetDontShowDataEffect(this))
			//{
			//	...
			//}
			//else
			//{
			//	...
			//}

			for (int i = 0; i < codes.Count; i++)
			{
				if (codes[i].opcode == OpCodes.Ldfld && ((FieldInfo)codes[i].operand).Name == "isGhost_")
				{
					Mod.Instance.Logger.Info($"ldfld isGhost_ @ {i}");

					// Replace: ldfld isGhost_
					// With:    call Mod.GetDontShowDataEffect
					codes[i].opcode = OpCodes.Call;
					codes[i].operand = typeof(Mod).GetMethod(nameof(Mod.GetDontShowDataEffect));

					break;
				}
			}

			return codes.AsEnumerable();
		}
	}
}
