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
	/// Patch the replay options menu slider to allow a maximum ghost count up to <see cref="Mod.MaxReplaysAtAll"/>.
	/// </summary>
	[HarmonyPatch(typeof(ReplayOptionsMenu), nameof(ReplayOptionsMenu.InitializeVirtual))]
	internal static class ReplayOptionsMenu__InitializeVirtual
	{
		// override void InitializeVirtual()
		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			// VISUAL:
			// Changes the max slider value parameter (20) to the new maximum specified by the mod.
			//__instance.TweakInt("GHOSTS IN ARCADE COUNT", __instance.settings_.GhostsInArcadeCount_, 1, 20, 5, delegate (int x)
			//{
			//	__instance.settings_.GhostsInArcadeCount_ = x;
			//}, null);

			var codes = new List<CodeInstruction>(instructions);
			for (int i = 0; i < codes.Count; i++)
			{
				// The only instance of 20 appearing as an operand in this function is for the max comparison.
				if ((i > 5) &&
					(codes[i    ].opcode == OpCodes.Ldc_I4_S && codes[i    ].operand.ToString() == "20") &&
					(codes[i - 5].opcode == OpCodes.Ldstr    && codes[i - 5].operand.ToString().ToUpperInvariant() == "GHOSTS IN ARCADE COUNT"))
				{

					codes[i].opcode = OpCodes.Ldc_I4;
					codes[i].operand = (int)Math.Max(Mod.MaxReplaysAtAll, 20);

					break;
				}
			}
			return codes.AsEnumerable();
		}


		// Prefix patch implementation:
		/*internal static bool Prefix(ReplayOptionsMenu __instance)
		{
			__instance.settings_ = G.Sys.OptionsManager_.Replay_;

			KeyValuePair<string, ReplaySettings.GhostsInArcade>[] entries = (!ReplayManager.SupportsOnlineReplays_) ? ReplayOptionsMenu.offlineGIAOptions_ : ReplayOptionsMenu.allGIAOptions_;
			
			__instance.TweakEnum<ReplaySettings.GhostsInArcade>("GHOSTS IN ARCADE TYPE", () => __instance.settings_.GhostsInArcadeType_, delegate (ReplaySettings.GhostsInArcade x)
			{
				__instance.settings_.GhostsInArcadeType_ = x;
			}, entries);

			// Change slider limit from 20 to new maximum.
			int max = Math.Max(Mod.MaxReplaysAtAll, 20);
			__instance.TweakInt("GHOSTS IN ARCADE COUNT", __instance.settings_.GhostsInArcadeCount_, 1, max, 5, delegate (int x)
			{
				__instance.settings_.GhostsInArcadeCount_ = x;
			}, null);

			__instance.TweakFloat("GHOST BRIGHTNESS", __instance.settings_.GhostBrightness_, 0.05f, 1f, 0.75f, delegate (float x)
			{
				__instance.settings_.GhostBrightness_ = x;
			}, null);
			__instance.TweakBool("GHOST NAMES VISIBLE", __instance.settings_.GhostsNamesVisible_, delegate (bool x)
			{
				__instance.settings_.GhostsNamesVisible_ = x;
			}, null);
			__instance.TweakBool("PLAYBACK SPEED AFFECTS MUSIC", __instance.settings_.PlaybackSpeedAffectsMusic_, delegate (bool x)
			{
				__instance.settings_.PlaybackSpeedAffectsMusic_ = x;
			}, null);


			// Skip the original method. We've performed everything it would have.
			return false;
		}*/
	}
}
