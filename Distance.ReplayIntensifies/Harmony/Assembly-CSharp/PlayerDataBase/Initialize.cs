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
	/// Patch to allow unclamped car colors for opponents (so others can show off their ultrabright car presets).
	/// This patch is needed because the <see cref="PlayerDataBase.ClampCarColors_"/> property seems to be inlined
	/// by the JIT.
	/// </summary>
	/// <remarks>
	/// Required For: Unrestricted Opponent Car Colors.
	/// </remarks>
	[HarmonyPatch(typeof(PlayerDataBase), nameof(PlayerDataBase.Initialize))]
	internal static class PlayerDataBase__Initialize
	{
		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			Mod.Instance.Logger.Info("Transpiling...");
			// VISUAL:
			// Replace call to PlayerDataBase.ClampCarColors_.get with Mod.GetClampCarColors
			//if (PlayerDataBase.ClampCarColors_)
			//{
			//	data.carColors_.ClampColors();
			//}

			var codes = new List<CodeInstruction>(instructions);
			for (int i = 0; i < codes.Count; i++)
			{
				// Find getter for: virtual bool PlayerDataBase.ClampCarColors_
				if (codes[i].opcode == OpCodes.Callvirt && ((MethodInfo)codes[i].operand).Name == "get_ClampCarColors_")
				{
					Mod.Instance.Logger.Info($"callvirt get_ClampCarColors_ @ {i}");

					// Replace: callvirt get_ClampCarColors_
					// With:    call Mod.GetClampCarColors
					codes[i].opcode = OpCodes.Call;
					codes[i].operand = typeof(Mod).GetMethod(nameof(Mod.GetClampCarColors));

					break;
				}
			}
			return codes.AsEnumerable();
		}
	}
}
