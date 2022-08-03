using Distance.ReplayIntensifies.Scripts;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Distance.ReplayIntensifies.Harmony
{
	/// <summary>
	/// Patch to allow unclamped car colors for opponents (so others can show off their ultrabright car presets).
	/// This patch is needed because the <see cref="PlayerDataBase.ClampCarColors_"/> property seems to be inlined
	/// by the JIT.
	/// <para/>
	/// Also includes patch to initialized randomized car type and colors.
	/// </summary>
	/// <remarks>
	/// Required For: Unrestricted Opponent Car Colors, and Randomized Cars.
	/// </remarks>
	[HarmonyPatch(typeof(PlayerDataBase), nameof(PlayerDataBase.Initialize))]
	internal static class PlayerDataBase__Initialize
	{
		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var codes = new List<CodeInstruction>(instructions);

			Mod.Instance.Logger.Info("Transpiling... (1/3)");
			// VISUAL:
			// Replace call to PlayerDataBase.ClampCarColors_.get with Mod.GetClampCarColors
			//if (PlayerDataBase.ClampCarColors_)
			//{
			//	data.carColors_.ClampColors();
			//}

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

			Mod.Instance.Logger.Info("Transpiling... (2/3)");
			// VISUAL:
			// Override call to assigning car type so we can hook random car selection.
			//this.UseCarWithName(data.carName_, null);
			// -to-
			//UseCarWithNameOverride_(this, data.carName_, null);

			for (int i = 0; i < codes.Count; i++)
			{
				if (codes[i].opcode == OpCodes.Call && ((MethodInfo)codes[i].operand).Name == "UseCarWithName")
				{
					Mod.Instance.Logger.Info($"call UseCarWithName @ {i}");

					// Replace: call UseCarWithName
					// With:    call UseCarWithNameOverride_
					codes[i].opcode = OpCodes.Call;
					codes[i].operand = typeof(PlayerDataBase__Initialize).GetMethod(nameof(UseCarWithNameOverride_));

					break;
				}
			}

			Mod.Instance.Logger.Info("Transpiling... (3/3)");
			// VISUAL:
			// Override call to assigning car colors so we can hook random car selection.
			//this.SetOriginalColors(data.carColors_);
			// -to-
			//SetOriginalColorsOverride_(this, data.carColors_);

			for (int i = 0; i < codes.Count; i++)
			{
				if (codes[i].opcode == OpCodes.Call && ((MethodInfo)codes[i].operand).Name == "SetOriginalColors")
				{
					Mod.Instance.Logger.Info($"call SetOriginalColors @ {i}");

					// Replace: call SetOriginalColors
					// With:    call SetOriginalColorsOverride_
					codes[i].opcode = OpCodes.Call;
					codes[i].operand = typeof(PlayerDataBase__Initialize).GetMethod(nameof(SetOriginalColorsOverride_));

					break;
				}
			}

			return codes.AsEnumerable();
		}

		#region Helpers

		public static bool UseCarWithNameOverride_(PlayerDataBase playerDataBase, string carName, Profile profile)
		{
			if (playerDataBase is PlayerDataReplay playerDataReplay)
			{
				var compoundData = playerDataReplay.GetComponent<PlayerDataReplayCompoundData>();
				if (compoundData && compoundData.IsRandomnessEnabled)
				{
					carName = compoundData.CarData.name_;
				}
			}
			return playerDataBase.UseCarWithName(carName, profile);
		}

		public static void SetOriginalColorsOverride_(PlayerDataBase playerDataBase, CarColors carColors)
		{
			if (playerDataBase is PlayerDataReplay playerDataReplay)
			{
				var compoundData = playerDataReplay.GetComponent<PlayerDataReplayCompoundData>();
				if (compoundData && compoundData.IsRandomnessEnabled)
				{
					carColors = compoundData.CarData.colors_;
				}
			}
			playerDataBase.SetOriginalColors(carColors);
		}

		#endregion
	}
}
