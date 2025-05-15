using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Distance.ReplayIntensifies.Patches
{
	/// <summary>
	/// Patch to handle ghost visual setup for simple exploder and jet flame color.
	/// <para/>
	/// Separates handling of ghost mode and ghost visual states.
	/// </summary>
	/// <remarks>
	/// Required For: Car Visual Style (part 5/7).
	/// </remarks>
	[HarmonyPatch(typeof(PlayerDataReplay), nameof(PlayerDataReplay.InitCarVirtual))]
	internal static class PlayerDataReplay__InitCarVirtual
	{
		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var codes = new List<CodeInstruction>(instructions);

			Mod.Log.LogInfo("Transpiling...");
			// VISUAL:
			// Change ghost setup for jet flame color and exploder to only occur for visual ghosts.
			//if (this.isGhost_)
			// -to-
			//if (Mod.GetIsGhostVisual(this))
			//{
			//	this.carObj_.GetComponent<CarExploder>().Simple_ = true;
			//	this.SetJetFlameColor();
			//}

			for (int i = 0; i < codes.Count; i++)
			{
				if (codes[i].opcode == OpCodes.Ldfld && ((FieldInfo)codes[i].operand).Name == "isGhost_")
				{
					Mod.Log.LogInfo($"ldfld isGhost_ @ {i}");

					// Replace: ldfld isGhost_
					// With:    call Mod.GetIsGhostVisual
					codes[i].opcode = OpCodes.Call;
					codes[i].operand = typeof(Mod).GetMethod(nameof(Mod.GetIsGhostVisual));

					break;
				}
			}

			return codes.AsEnumerable();
		}
	}
}
